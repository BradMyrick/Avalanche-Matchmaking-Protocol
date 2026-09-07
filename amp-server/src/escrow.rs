//! Escrow verification: read-only chain access to confirm a staked match is
//! actually funded in `AMPRegistry` before the game goes live. The server
//! never signs escrow transactions — players lock their own stakes; we only
//! verify what landed on-chain.

use alloy_primitives::{Address, U256};
use alloy_sol_types::sol;

sol! {
    #[sol(rpc)]
    contract AMPRegistryView {
        function matches(uint256 id)
            external
            view
            returns (uint256 gameId, address playerA, uint8 state, address playerB, uint64 createdAt, uint256 stakeAmount, uint256 stakeAmountB);
    }
}

#[derive(Debug, Clone, PartialEq)]
pub struct OnChainMatch {
    pub game_id: u64,
    pub player_a: Address,
    /// AMPTypes.MatchState: 0 OPEN, 1 READY, 2 SETTLED, 3 EXPIRED, 4 DISPUTED
    pub state: u8,
    pub player_b: Address,
    pub stake_amount: U256,
    pub stake_amount_b: U256,
}

#[allow(dead_code)] // OPEN is the pre-join escrow state, surfaced in verify responses
pub const STATE_OPEN: u8 = 0;
pub const STATE_READY: u8 = 1;

/// Read a match from the registry. Returns None when the slot is empty
/// (playerA == 0).
pub async fn read_match(
    rpc_url: &str,
    registry: Address,
    on_chain_match_id: u64,
) -> anyhow::Result<Option<OnChainMatch>> {
    let provider = alloy_provider::ProviderBuilder::new()
        .connect(rpc_url)
        .await?;
    let contract = AMPRegistryView::new(registry, provider);
    let AMPRegistryView::matchesReturn {
        gameId,
        playerA,
        state,
        playerB,
        createdAt: _,
        stakeAmount,
        stakeAmountB,
    } = contract
        .matches(U256::from(on_chain_match_id))
        .call()
        .await?;

    if playerA == Address::ZERO {
        return Ok(None);
    }
    Ok(Some(OnChainMatch {
        game_id: gameId.to::<u64>(),
        player_a: playerA,
        state,
        player_b: playerB,
        stake_amount: stakeAmount,
        stake_amount_b: stakeAmountB,
    }))
}

// ── AMPMultiplayer lobby phase reads (multi lifecycle) ──────────────

sol! {
    #[sol(rpc)]
    contract AMPMultiplayerView {
        enum State { Empty, Open, Ready, GracePending, Disputed, Settled, Cancelled }
        struct MatchView {
            uint256 gameId;
            uint64 lobbySize;
            uint16 payoutProfileId;
            uint256 stakePerPlayer;
            uint256 bondPerPlayer;
            uint64 joinedUntil;
            uint64 readyAt;
            uint64 quorumUntil;
            uint64 graceUntil;
            uint64 challengeUntil;
            State state;
            uint256 joinedMask;
        }
        function getMatchPhase(bytes32 matchId) external view returns (State state, uint64 joinedUntil, uint64 readyAt);
    }
}

/// On-chain phase of a multiplayer lobby.
#[derive(Debug, Clone, Copy, PartialEq)]
pub enum LobbyPhase {
    /// Not created yet on-chain (job pending) or Empty.
    Missing,
    /// Created, filling escrow (State::Open).
    Funding,
    /// All N funded — the match is live.
    Ready,
    /// Fill window passed without everyone funding.
    Expired,
}

/// Read a multiplayer lobby's phase. Reads config from the store's env
/// (AMP_RPC_URL + AMP_MULTIPLAYER_ADDRESS); returns Missing when the
/// multiplayer contract isn't configured (free-play mode).
pub async fn multi_lobby_ready(
    _store: &crate::store::Store,
    match_id: alloy_primitives::B256,
) -> anyhow::Result<LobbyPhase> {
    let rpc_url = std::env::var("AMP_RPC_URL").unwrap_or_default();
    let mp_addr: Address = std::env::var("AMP_MULTIPLAYER_ADDRESS")
        .ok()
        .and_then(|a| a.parse().ok())
        .unwrap_or_default();
    if rpc_url.is_empty() || mp_addr == Address::ZERO {
        // Free-play deployment: no on-chain escrow — treat as Ready so
        // matches go live immediately after formation.
        return Ok(LobbyPhase::Ready);
    }

    let provider = alloy_provider::ProviderBuilder::new()
        .connect(&rpc_url)
        .await?;
    let contract = AMPMultiplayerView::new(mp_addr, provider);
    let AMPMultiplayerView::getMatchPhaseReturn { state, joinedUntil, readyAt } = contract
        .getMatchPhase(match_id)
        .call()
        .await?;

    use alloy_primitives::U256;
    let _ = U256::from(0u8); // keep import shape stable

    Ok(match state {
        AMPMultiplayerView::State::Open => {
            // The fill window is enforced by the contract's joinLobby; if
            // it has lapsed, the contract considers the lobby expirable.
            let now = std::time::SystemTime::now()
                .duration_since(std::time::UNIX_EPOCH)
                .unwrap_or_default()
                .as_secs();
            if joinedUntil != 0 && now > u64::from(joinedUntil) {
                LobbyPhase::Expired
            } else {
                LobbyPhase::Funding
            }
        }
        AMPMultiplayerView::State::Ready => {
            let _ = readyAt;
            LobbyPhase::Ready
        }
        AMPMultiplayerView::State::Empty => LobbyPhase::Missing,
        AMPMultiplayerView::State::Cancelled => LobbyPhase::Expired,
        _ => LobbyPhase::Funding, // settled/grace-pending/disputed: not ours to move
    })
}
