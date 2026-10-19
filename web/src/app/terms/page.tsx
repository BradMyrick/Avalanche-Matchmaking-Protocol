import Link from "next/link";

export default function TermsPage() {
  return (
    <div className="relative min-h-screen overflow-hidden antialiased bg-black text-white">
      <div className="absolute top-0 -left-1/4 w-[150%] h-[500px] bg-brand-cyan/10 blur-[120px] rounded-full pointer-events-none" />
      <div className="absolute bottom-0 -right-1/4 w-[150%] h-[500px] bg-brand-red/10 blur-[120px] rounded-full pointer-events-none" />

      <header className="relative z-10 max-w-3xl mx-auto px-6 pt-10">
        <Link href="/" className="inline-flex items-center gap-2 text-zinc-400 hover:text-brand-cyan transition-colors text-sm">
          &larr; Back to AMP
        </Link>
      </header>

      <main className="relative z-10 max-w-3xl mx-auto px-6 py-10">
        <h1 className="text-3xl font-black uppercase tracking-tight mb-2">
          Terms &amp; <span className="text-brand-cyan">Disclaimers</span>
        </h1>
        <p className="text-zinc-500 text-sm mb-10">Last updated: August 2026</p>

        <div className="space-y-8 text-zinc-300 leading-relaxed">
          <section>
            <h2 className="text-lg font-bold text-white uppercase tracking-wide mb-2">1. Skill-Based Competition — Not Gambling</h2>
            <p className="text-sm">
              AMP tournaments are skill-based competitive events. Outcomes are determined by player performance, not chance.
              AMP is not a gambling platform, casino, or betting service. Prize pools are funded by sponsors and paid to
              winners based on verifiable match results. Participants are responsible for complying with their local laws
              regarding skill-based competitions.
            </p>
          </section>

          <section>
            <h2 className="text-lg font-bold text-white uppercase tracking-wide mb-2">2. Testnet — No Real Value</h2>
            <p className="text-sm">
              The protocol currently operates on the Avalanche Fuji testnet. All tokens (AVAX, ERC-20s) used on this
              platform are testnet tokens with no monetary value. No real funds are at risk. Mainnet deployment will be
              announced separately with updated terms.
            </p>
          </section>

          <section>
            <h2 className="text-lg font-bold text-white uppercase tracking-wide mb-2">3. Protocol Fee</h2>
            <p className="text-sm">
              A 2% protocol fee is deducted from each prize payout and sent to the protocol treasury wallet
              (<code className="text-brand-cyan">0x95CC…077a</code>). This fee covers infrastructure costs (database,
              hosting, relayer operations). Winners receive their prize minus the 2% fee. The fee rate is governed by the
              contract owner and is publicly verifiable on-chain.
            </p>
          </section>

          <section>
            <h2 className="text-lg font-bold text-white uppercase tracking-wide mb-2">4. Crypto Transactions Are Irreversible</h2>
            <p className="text-sm">
              All on-chain transactions (funding prize pools, claiming prizes) are final and irreversible. AMP cannot
              reverse, refund, or modify a transaction once it is confirmed on Avalanche. Users must verify addresses and
              amounts before submitting any transaction.
            </p>
          </section>

          <section>
            <h2 className="text-lg font-bold text-white uppercase tracking-wide mb-2">5. No Warranty — Software Provided As-Is</h2>
            <p className="text-sm">
              The AMP software is open-source (Apache 2.0) and provided &ldquo;as is&rdquo; without warranty of any kind,
              express or implied, including but not limited to the warranties of merchantability, fitness for a particular
              purpose, and non-infringement. The developers make no guarantee that the software will be error-free,
              uninterrupted, or secure.
            </p>
          </section>

          <section>
            <h2 className="text-lg font-bold text-white uppercase tracking-wide mb-2">6. User Responsibility</h2>
            <p className="text-sm">
              Users are solely responsible for: (a) maintaining the security of their private keys and wallets;
              (b) complying with all applicable laws in their jurisdiction, including tax reporting obligations for any
              prizes received; (c) providing accurate information when funding tournaments or claiming prizes.
            </p>
          </section>

          <section>
            <h2 className="text-lg font-bold text-white uppercase tracking-wide mb-2">7. Privacy</h2>
            <p className="text-sm">
              Wallet addresses used on AMP are public on the Avalanche blockchain. PayPal transactions are processed by
              PayPal and subject to PayPal&rsquo;s privacy policy. AMP stores tournament data (brackets, results, prize
              amounts) in its database but does not sell or share user data with third parties.
            </p>
          </section>

          <section>
            <h2 className="text-lg font-bold text-white uppercase tracking-wide mb-2">8. Limitation of Liability</h2>
            <p className="text-sm">
              To the maximum extent permitted by law, the AMP developers and contributors shall not be liable for any
              indirect, incidental, special, consequential, or punitive damages, or any loss of profits or revenues,
              whether incurred directly or indirectly, or any loss of data, use, or goodwill, arising out of or related
              to the use of the AMP software or platform.
            </p>
          </section>

          <section>
            <h2 className="text-lg font-bold text-white uppercase tracking-wide mb-2">9. Contact</h2>
            <p className="text-sm">
              Questions? Email <a href="mailto:brad@kodr.pro" className="text-brand-cyan hover:underline">brad@kodr.pro</a>.
            </p>
          </section>

          <section>
            <h2 className="text-lg font-bold text-white uppercase tracking-wide mb-2">10. Open Source</h2>
            <p className="text-sm">
              AMP is licensed under the Apache License, Version 2.0. The full source code is available at{" "}
              <a href="https://github.com/BradMyrick/Avalanche-Matchmaking-Protocol" target="_blank" rel="noreferrer" className="text-brand-cyan hover:underline">
                github.com/BradMyrick/Avalanche-Matchmaking-Protocol
              </a>.
            </p>
          </section>
        </div>

        <div className="mt-12 pt-8 border-t border-white/10">
          <p className="text-xs text-zinc-600">
            © 2026 AMP — Avalanche Matchmaking Protocol. Backed by Avalanche Build Games 2026.
          </p>
        </div>
      </main>
    </div>
  );
}
