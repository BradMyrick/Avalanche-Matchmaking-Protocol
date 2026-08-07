import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "AMP - Verifiable Tournament Engine on Avalanche",
  description:
    "Run trustless tournaments with escrowed prize pools, instant payouts, and tamper-proof results. Any game. Any engine. Any community. Settle champions on Avalanche.",
  icons: {
    apple: "/amp-icon.png",
  },
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en" suppressHydrationWarning>
      <body className="antialiased">
        {children}
      </body>
    </html>
  );
}
