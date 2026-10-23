import React from "react";
import { AbsoluteFill, interpolate, useCurrentFrame } from "remotion";
import { useLayout } from "../layout";
import { colors, fontFamily } from "../theme";

export type GlobalSentence = { text: string; from: number; to: number };

/**
 * O anda okunan cümleyi alt kısımda yarı saydam kutuda gösterir.
 * Cümleler arasındaki kısa sessizlikte önceki cümle ekranda kalır (titreme olmasın).
 */
export const Subtitles: React.FC<{ sentences: GlobalSentence[] }> = ({ sentences }) => {
  const frame = useCurrentFrame();
  const { vertical, subtitleBottom } = useLayout();

  const index = sentences.findIndex((s, i) => {
    const next = sentences[i + 1];
    const holdUntil = next && next.from - s.to < 20 ? next.from : s.to + 8;
    return frame >= s.from && frame < holdUntil;
  });
  if (index < 0) return null;
  const s = sentences[index];
  const prev = sentences[index - 1];
  const continued = prev && s.from - prev.to < 20; // önceki cümleden kesintisiz geçiş: yeniden belirme efekti yok
  const opacity = continued ? 1 : interpolate(frame - s.from, [0, 5], [0, 1], { extrapolateRight: "clamp" });

  return (
    <AbsoluteFill style={{ justifyContent: "flex-end", alignItems: "center", pointerEvents: "none" }}>
      <div
        style={{
          marginBottom: subtitleBottom,
          maxWidth: vertical ? 940 : 1500,
          padding: vertical ? "22px 32px" : "18px 36px",
          borderRadius: 22,
          background: "rgba(3, 12, 11, 0.72)",
          border: "1px solid rgba(255,255,255,.08)",
          backdropFilter: "blur(8px)",
          color: "#ffffff",
          fontFamily,
          fontWeight: 600,
          fontSize: vertical ? 50 : 46,
          lineHeight: 1.3,
          textAlign: "center",
          textShadow: "0 2px 6px rgba(0,0,0,.5)",
          opacity,
          boxShadow: `0 10px 40px rgba(0,0,0,.35), inset 0 -3px 0 ${colors.brand}`,
        }}
      >
        {s.text}
      </div>
    </AbsoluteFill>
  );
};
