import React from "react";
import { AbsoluteFill, Img, staticFile, useCurrentFrame } from "remotion";
import { ease, pop } from "../anim";
import { DownloadIcon } from "../components/Icons";
import { Backdrop } from "../components/Ui";
import { useLayout } from "../layout";
import { SceneProps } from "../timeline";
import { colors } from "../theme";

export const DOWNLOAD_URL = "swlandofficial.github.io/EkranSecici";

export const Outro: React.FC<SceneProps> = ({ sentences }) => {
  const frame = useCurrentFrame();
  const { vertical, content } = useLayout();
  const s1 = sentences[1] ?? sentences[0];
  const logo = pop(frame, 0);
  const btn = pop(frame, 12);
  const tag = ease(frame, s1.from - 4);
  return (
    <Backdrop glow="50% 45%">
      <AbsoluteFill
        style={{
          top: content.top,
          height: content.bottom - content.top,
          display: "flex",
          flexDirection: "column",
          alignItems: "center",
          justifyContent: "center",
          gap: vertical ? 34 : 26,
        }}
      >
        <div style={{ display: "flex", alignItems: "center", gap: 28, transform: `scale(${logo})` }}>
          <Img src={staticFile("logo.png")} style={{ width: vertical ? 150 : 140 }} />
          <div style={{ fontSize: vertical ? 92 : 100, fontWeight: 900, letterSpacing: -3 }}>Ekran Seçici</div>
        </div>
        <div style={{ fontSize: vertical ? 48 : 50, fontWeight: 700, color: colors.brandBright, opacity: tag }}>
          Her program, doğru ekranda.
        </div>
        <div
          style={{
            marginTop: 20,
            display: "flex",
            alignItems: "center",
            gap: 18,
            fontSize: vertical ? 44 : 42,
            fontWeight: 800,
            background: colors.brandBright,
            color: colors.onBrand,
            padding: "24px 44px",
            borderRadius: 22,
            transform: `scale(${btn})`,
            boxShadow: "0 20px 60px rgba(45,212,191,.35)",
          }}
        >
          <DownloadIcon size={46} color={colors.onBrand} strokeWidth={2.6} /> Ücretsiz indir
        </div>
        <div style={{ fontSize: vertical ? 36 : 36, fontWeight: 600, color: colors.text, opacity: btn }}>{DOWNLOAD_URL}</div>
        <div style={{ fontSize: 24, color: colors.muted, opacity: btn }}>SWLand · Windows 10 / 11</div>
      </AbsoluteFill>
    </Backdrop>
  );
};
