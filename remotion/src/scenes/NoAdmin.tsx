import React from "react";
import { AbsoluteFill, useCurrentFrame } from "remotion";
import { ease, pop } from "../anim";
import { CheckIcon, ShieldIcon } from "../components/Icons";
import { Backdrop, FeatureHeader } from "../components/Ui";
import { useLayout } from "../layout";
import { SceneProps } from "../timeline";
import { colors } from "../theme";

/** Özellik 5: yönetici yetkisi ve ek kurulum gerektirmez. */
export const NoAdmin: React.FC<SceneProps> = ({ sentences }) => {
  const frame = useCurrentFrame();
  const { vertical, content } = useLayout();
  const s0 = sentences[0];
  const s1 = sentences[1] ?? s0;
  const items = [
    { text: "Yönetici şifresi gerekmez", at: s0.from },
    { text: "Ek kurulum (.NET) gerekmez", at: s0.from + (s0.to - s0.from) * 0.5 },
    { text: "Domain bilgisayarlarında çalışır", at: s1.from },
  ];
  const shield = pop(frame, 4);
  const path = ease(frame, s1.from + 24);

  return (
    <Backdrop glow={vertical ? "50% 35%" : "25% 50%"}>
      <AbsoluteFill
        style={{
          top: content.top,
          left: content.left,
          width: content.right - content.left,
          height: content.bottom - content.top,
          display: "flex",
          flexDirection: "column",
          alignItems: vertical ? "center" : "stretch",
          gap: vertical ? 60 : 50,
        }}
      >
        <FeatureHeader index={5} title="Yetki istemez, kolay kurulur" size={vertical ? 54 : 60} />
        <div
          style={{
            flex: 1,
            display: "flex",
            flexDirection: vertical ? "column" : "row",
            alignItems: "center",
            justifyContent: "center",
            gap: vertical ? 60 : 110,
          }}
        >
          <div
            style={{
              width: vertical ? 300 : 340,
              height: vertical ? 300 : 340,
              borderRadius: 999,
              background: "rgba(45,212,191,.12)",
              border: `3px solid ${colors.brandBright}`,
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              transform: `scale(${shield})`,
            }}
          >
            <ShieldIcon size={vertical ? 170 : 190} color={colors.brandBright} strokeWidth={1.6} />
          </div>
          <div>
            {items.map((it) => {
              const t = ease(frame, it.at - 3, 14);
              return (
                <div
                  key={it.text}
                  style={{
                    display: "flex",
                    alignItems: "center",
                    gap: 22,
                    fontSize: vertical ? 40 : 44,
                    fontWeight: 700,
                    marginBottom: 30,
                    opacity: t,
                    transform: `translateX(${(1 - t) * 40}px)`,
                  }}
                >
                  <div style={{ width: 58, height: 58, borderRadius: 999, background: colors.brandBright, display: "flex", alignItems: "center", justifyContent: "center", flexShrink: 0 }}>
                    <CheckIcon size={36} color={colors.onBrand} strokeWidth={3} />
                  </div>
                  {it.text}
                </div>
              );
            })}
            <div
              style={{
                marginTop: 14,
                display: "inline-block",
                fontFamily: "monospace",
                fontSize: vertical ? 30 : 32,
                padding: "12px 20px",
                borderRadius: 12,
                background: colors.surface,
                border: `2px solid ${colors.line}`,
                color: colors.brandSoft,
                opacity: path,
              }}
            >
              %APPDATA%\EkranSecici
            </div>
          </div>
        </div>
      </AbsoluteFill>
    </Backdrop>
  );
};
