import React from "react";
import { AbsoluteFill, Img, staticFile, useCurrentFrame } from "remotion";
import { ease, pop } from "../anim";
import { Backdrop } from "../components/Ui";
import { useLayout } from "../layout";
import { SceneProps } from "../timeline";
import { colors } from "../theme";

export const Reveal: React.FC<SceneProps> = () => {
  const frame = useCurrentFrame();
  const { vertical, content } = useLayout();
  const logo = pop(frame, 2);
  const name = ease(frame, 10);
  const tag = ease(frame, 22);
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
          gap: 28,
        }}
      >
        <Img src={staticFile("logo.png")} style={{ width: vertical ? 260 : 230, transform: `scale(${logo})` }} />
        <div
          style={{
            fontSize: vertical ? 120 : 130,
            fontWeight: 900,
            letterSpacing: -4,
            opacity: name,
            transform: `translateY(${(1 - name) * 30}px)`,
          }}
        >
          Ekran Seçici
        </div>
        <div style={{ fontSize: vertical ? 50 : 52, fontWeight: 700, color: colors.brandBright, opacity: tag }}>
          Her program, doğru ekranda.
        </div>
      </AbsoluteFill>
    </Backdrop>
  );
};
