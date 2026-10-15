const KEYWORDS = new Set([
  "using", "public", "class", "async", "void", "var", "if", "await", "return", "new",
  "import", "from", "const", "let", "function", "interface", "external", "payable",
  "returns", "string", "uint256", "address", "bytes", "uint16", "uint64", "bool",
  "true", "false", "null", "this", "for", "else", "include", "int32",
]);

type Kind = "comment" | "string" | "keyword" | "number" | "plain";

function tokenize(code: string): { text: string; kind: Kind }[] {
  const re =
    /((?:^[ \t]*#(?!include).*)$|\/\/.*$|"(?:\\.|[^"\\])*"|'(?:\\.|[^'\\])*'|`(?:\\.|[^`\\])*`|\b0x[0-9a-fA-F]+\b|\b\d+(?:\.\d+)?\b|\b[A-Za-z_][\w]*\b)/gm;
  const out: { text: string; kind: Kind }[] = [];
  let last = 0;
  for (const match of code.matchAll(re)) {
    const index = match.index ?? 0;
    if (index > last) out.push({ text: code.slice(last, index), kind: "plain" });
    const text = match[0];
    let kind: Kind = "plain";
    if (text.startsWith("//") || /^\s*#(?!include)/.test(text)) kind = "comment";
    else if (text.startsWith('"') || text.startsWith("'") || text.startsWith("`")) kind = "string";
    else if (text.startsWith("0x") || /^\d/.test(text)) kind = "number";
    else if (KEYWORDS.has(text)) kind = "keyword";
    out.push({ text, kind });
    last = index + text.length;
  }
  if (last < code.length) out.push({ text: code.slice(last), kind: "plain" });
  return out;
}

const color: Record<Kind, string> = {
  comment: "text-zinc-500",
  string: "text-yellow-300",
  keyword: "text-brand-cyan",
  number: "text-yellow-400",
  plain: "text-zinc-200",
};

export default function CodeSample({ code, className = "" }: { code: string; className?: string }) {
  return (
    <pre className={`font-mono text-xs sm:text-sm leading-relaxed whitespace-pre overflow-x-auto ${className}`}>
      <code>
        {tokenize(code).map((token, i) => (
          <span key={i} className={color[token.kind]}>
            {token.text}
          </span>
        ))}
      </code>
    </pre>
  );
}
