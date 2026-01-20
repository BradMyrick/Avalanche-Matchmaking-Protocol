import { ethers } from "ethers";

/**
 * Result signed by an async verifier. 
 * Matches the Solidity struct in AMPTypes.sol.
 */
export interface AsyncResult {
    matchId: bigint;
    outcome: number;
    transcriptHash: string;
    signature: string;
}

/**
 * Result submitted by a player for real-time agreement.
 */
export interface RealTimeHashResult {
    matchId: bigint;
    outcome: number;
    transcriptHash: string;
}

export interface AMPClientConfig {
    provider: ethers.Provider | ethers.Signer;
    registryAddress: string;
    settlementAddress: string;
}

/**
 * AMPClient SDK for interacting with the AMP protocol.
 */
export class AMPClient {
    private registry: ethers.Contract;
    private settlement: ethers.Contract;

    constructor(private config: AMPClientConfig) {
        // TODO: Load ABIs from compiled contract artifacts.
        const genericAbi: string[] = [];
        this.registry = new ethers.Contract(config.registryAddress, genericAbi, config.provider);
        this.settlement = new ethers.Contract(config.settlementAddress, genericAbi, config.provider);
    }

    /**
     * Creates a new match on the AMP registry.
     */
    async createMatch(opts: { gameId: number; stakeAmount: bigint }): Promise<number> {
        // TODO: call AMPRegistry.createMatch via ethers.
        // const tx = await this.registry.createMatch(opts.gameId, { value: opts.stakeAmount });
        // const receipt = await tx.wait();
        // TODO: parse tx receipt, return matchId from events.
        console.log("createMatch called with:", opts);
        return 0; // Placeholder
    }

    /**
     * Joins an existing OPEN match.
     */
    async joinMatch(matchId: number, stakeAmount: bigint): Promise<void> {
        // TODO: call AMPRegistry.joinMatch.
        // await this.registry.joinMatch(matchId, { value: stakeAmount });
        console.log("joinMatch called for match:", matchId);
    }

    /**
     * Submits a signed result from an async verifier to the settlement contract.
     */
    async submitAsyncResult(matchId: number, result: AsyncResult): Promise<void> {
        // TODO: call AMPSettlement.submitAsyncResult.
        // await this.settlement.submitAsyncResult(matchId, result);
        console.log("submitAsyncResult called for match:", matchId);
    }

    /**
     * Submits a result hash for real-time agreement settlement.
     */
    async submitRealTimeHashResult(matchId: number, result: RealTimeHashResult): Promise<void> {
        // TODO: call AMPSettlement.submitRealTimeHashResult.
        // await this.settlement.submitRealTimeHashResult(matchId, result);
        console.log("submitRealTimeHashResult called for match:", matchId);
    }

    /**
     * Subscribes to the MatchSettled event.
     */
    onMatchSettled(handler: (matchId: bigint, outcome: number, payout: bigint) => void): void {
        // TODO: subscribe to MatchSettled event using provider.on or contract.on.
        // this.settlement.on("MatchSettled", handler);
        console.log("Subscribed to MatchSettled events");
    }
}
