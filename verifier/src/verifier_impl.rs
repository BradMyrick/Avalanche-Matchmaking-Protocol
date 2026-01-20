use crate::match_capnp::verifier;
use capnp::capability::Promise;
use capnp_rpc::pry;
use ethers_core::types::H256;
use ethers_core::utils::keccak256;

pub struct VerifierImpl;

impl verifier::Server for VerifierImpl {
    fn verify_async_replay(
        &mut self,
        params: verifier::VerifyAsyncReplayParams,
        mut results: verifier::VerifyAsyncReplayResults,
    ) -> Promise<(), capnp::Error> {
        let transcript = pry!(pry!(params.get()).get_transcript());
        let summary = pry!(transcript.get_summary());

        // TODO: parse transcript and re-simulate game.
        // For now, we derive the winner from the summary text.
        let summary_str = pry!(summary.to_str());
        let winner = if summary_str.contains("WIN_A") {
            "WIN_A"
        } else if summary_str.contains("WIN_B") {
            "WIN_B"
        } else {
            "DRAW"
        };

        let outcome_code = winner; // Simplified for MVP

        // TODO: Map summary to OutcomeCode enum values used in Solidity.

        // Compute resultHash as keccak256 of the summary (placeholder for actual transcript hash)
        let result_hash = keccak256(summary_str.as_bytes());

        let mut results_builder = results.get();
        results_builder.set_winner(winner);
        results_builder.set_outcome_code(outcome_code);
        results_builder.set_result_hash(&result_hash);

        Promise::ok(())
    }

    fn verify_real_time_transcript(
        &mut self,
        _params: verifier::VerifyRealTimeTranscriptParams,
        _results: verifier::VerifyRealTimeTranscriptResults,
    ) -> Promise<(), capnp::Error> {
        // TODO: implement real-time verification logic.
        Promise::ok(())
    }
}

// TODO: load private key from env and implement EIP-712 signing.
pub fn sign_result(
    match_id: u64,
    outcome: &str,
    result_hash: [u8; 32],
) -> Vec<u8> {
    // TODO: build EIP-712 or simple prefixed hash.
    // TODO: sign and return fields matching Solidity struct.

    // For now, return dummy signature bytes.
    vec![0u8; 65]
}
