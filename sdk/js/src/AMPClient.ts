import { ethers } from "ethers";
import { AMPRegistryABI } from "./contracts/AMPRegistryABI";
import { AMPSettlementABI } from "./contracts/AMPSettlementABI";

import { exec } from "child_process";
import { promisify } from "util";
const execAsync = promisify(exec);

export interface AsyncResult {
    matchId: bigint;
    outcome: number;
    transcriptHash: string;
    signature: string;
}

export interface RealTimeHashResult {
    matchId: bigint;
    outcome: number;
    transcriptHash: string;
}

export interface AMPClientConfig {
    provider: ethers.Provider | ethers.Signer;
    registryAddress: string;
    settlementAddress: string;
    matchmakerUrl: string;
}

export class AMPClient {
    public registry: ethers.Contract;
    public settlement: ethers.Contract;
    private playerAddress: string = "";
    private config: AMPClientConfig;

    constructor(config: AMPClientConfig) {
        this.config = config;
        this.registry = new ethers.Contract(config.registryAddress, AMPRegistryABI, config.provider);
        this.settlement = new ethers.Contract(config.settlementAddress, AMPSettlementABI, config.provider);
    }

    async connectMatchmaker(playerAddress: string): Promise<void> {
        this.playerAddress = playerAddress;
        // Handled per-request by the Rust CLI
    }

    async requestMatch(gameId: string): Promise<{ match_id: string, opponent_id: string }> {
        const cmd = `cargo run -q --manifest-path ../../mm-client/Cargo.toml -- request-match ${this.config.matchmakerUrl} ${gameId} ${this.playerAddress}`;
        const { stdout } = await execAsync(cmd);
        return { match_id: stdout.trim(), opponent_id: "" };
    }

    async submitOutcomeToMatchmaker(matchId: string, outcome: number, transcriptHash: string): Promise<{ match_id: string, signature: string }> {
        // dummy signature to reconnect
        const cmd = `cargo run -q --manifest-path ../../mm-client/Cargo.toml -- submit-outcome ${this.config.matchmakerUrl} ${matchId} ${outcome} ${this.playerAddress}`;
        const { stdout } = await execAsync(cmd);
        return { match_id: matchId, signature: stdout.trim() };
    }

    async createMatch(opts: { gameId: number; stakeAmount: bigint }): Promise<bigint> {
        const tx = await this.registry.createMatch(opts.gameId, { value: opts.stakeAmount });
        const receipt = await tx.wait();
        const event = receipt.logs.map((log: any) => {
            try { return this.registry.interface.parseLog(log); } catch { return null; }
        }).find((e: any) => e && e.name === "MatchCreated");
        return event ? event.args[0] : 0n;
    }

    async joinMatch(matchId: bigint, stakeAmount: bigint): Promise<void> {
        const tx = await this.registry.joinMatch(matchId, { value: stakeAmount });
        await tx.wait();
    }

    async submitAsyncResult(matchId: bigint, result: AsyncResult): Promise<void> {
        const tx = await this.settlement.submitAsyncResult(matchId, result);
        await tx.wait();
    }

    onMatchSettled(handler: (matchId: bigint, outcome: number, payout: bigint) => void): void {
        this.settlement.on("MatchSettled", handler);
    }
}
