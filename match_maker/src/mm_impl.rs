use crate::match_capnp::{game_connector, match_maker};
use capnp::capability::Promise;
use capnp_rpc::pry;
use ethers_core::utils::keccak256;

pub struct MMImpl;
pub struct GameConnector;

impl match_maker::Server for MMImpl {
    fn verify_async_replay(
        &mut self,
        params: match_maker::VerifyAsyncReplayParams,
        mut results: match_maker::VerifyAsyncReplayResults,
    ) -> Promise<(), capnp::Error> {
        let transcript = pry!(pry!(params.get()).get_transcript());
        let hash = pry!(transcript.get_hash());

        let mut results_builder = results.get();

        Promise::ok(())
    }

    fn verify_real_time_transcript(
        &mut self,
        _params: match_maker::VerifyRealTimeTranscriptParams,
        _results: match_maker::VerifyRealTimeTranscriptResults,
    ) -> Promise<(), capnp::Error> {
        // TODO: implement real-time verification logic.
        Promise::ok(())
    }
}

impl game_connector::Server for GameConnector {}

// TODO: load private key from env and implement EIP-712 signing.
pub fn sign_result(
    match_id: u64,
    outcome: &str,
    result_hash: [u8; 32],
) -> Vec<u8> {
    // TODO: build EIP-712 or simple prefixed hash.
    // TODO: sign and return fields matching Solidity struct.

    // For now, return dummy signature bytes.
    vec![0u8; 69]
}
