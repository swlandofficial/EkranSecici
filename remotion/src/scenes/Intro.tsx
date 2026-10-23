import React from "react";
import { AbsoluteFill, useCurrentFrame } from "remotion";
import { ease, lerp } from "../anim";
import { DualMonitors, Win } from "../components/DualMonitors";
import { Backdrop } from "../components/Ui";
import { useLayout } from "../layout";
import { SceneProps } from "../timeline";
import { colors } from "../theme";

/** Sorun: çağrı ekranı personelin ekranında, dış ekran boş. */
export const Intro: React.FC<SceneProps> = ({ sentences }) => {
  const frame = useCurrentFrame();
  const { vertical, content } = useLayout();
  const s2 = sentences[1]?.from ?? 9999;

  // İmleç pencereyi sürüklemeye çalışır, pencere geri kayar (bitmeyen uğraş).
  const loop = frame % 54;
  const drag = lerp(loop, [6, 26, 34, 46], [0, 0.14, 0.14, 0]);
  const shake = frame > s2 ? Math.sin(frame * 1.6) * 0.006 * lerp(frame, [s2, s2 + 8, s2 + 30], [0, 1, 0]) : 0;
  const winX = 0.42 + drag + shake;
  const windows: Win[] = [
    { id: "call", kind: "call", x: winX, y: 0.16, w: 0.46, h: 0.5, frame: true },
    { id: "cur", kind: "cursor", x: winX + 0.2, y: 0.18, pressed: loop > 6 && loop < 34 },
  ];

  const enter = ease(frame, 0, 24);
  const head = ease(frame, s2 - 4);
  return (
    <Backdrop glow="50% 55%">
      <AbsoluteFill
        style={{
          top: content.top,
          height: content.bottom - content.top,
          display: "flex",
          flexDirection: "column",
          alignItems: "center",
          justifyContent: "center",
          gap: vertical ? 40 : 50,
        }}
      >
        <div
          style={{
            fontSize: vertical ? 74 : 72,
            fontWeight: 900,
            letterSpacing: -2,
            textAlign: "center",
            lineHeight: 1.1,
            opacity: head,
            transform: `translateY(${(1 - head) * 30}px)`,
            minHeight: vertical ? 170 : 80,
          }}
        >
          Her sabah <span style={{ color: colors.warning }}>aynı uğraş</span> mı?
        </div>
        <div style={{ opacity: enter, transform: `translateY(${(1 - enter) * 60}px)` }}>
          <DualMonitors width={vertical ? 640 : 1400} stack={vertical} windows={windows} />
        </div>
      </AbsoluteFill>
    </Backdrop>
  );
};
