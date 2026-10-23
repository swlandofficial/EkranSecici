import React from "react";
import { useCurrentFrame } from "remotion";
import { ease, lerp } from "../anim";
import { DualMonitors, Win } from "../components/DualMonitors";
import { MagnetIcon, PinIcon, RefreshIcon } from "../components/Icons";
import { Split, usePanelSizes } from "../components/Split";
import { Backdrop, Card, FeatureHeader } from "../components/Ui";
import { SceneProps } from "../timeline";
import { colors } from "../theme";

const ITEMS = [
  { text: "Kapanırsa yeniden açılır", Icon: RefreshIcon },
  { text: "Kayarsa geri gelir", Icon: MagnetIcon },
  { text: "Her zaman en üstte", Icon: PinIcon },
];

/** Özellik 3: bekçi — yeniden aç, monitörde tut, en üstte tut. */
export const Watchdog: React.FC<SceneProps> = ({ sentences }) => {
  const frame = useCurrentFrame();
  const sizes = usePanelSizes();
  const [a, b, c] = [sentences[0], sentences[1] ?? sentences[0], sentences[2] ?? sentences[0]];
  const active = frame >= c.from ? 2 : frame >= b.from ? 1 : 0;

  // 1) Kapanır ve yeniden açılır
  const closeT = lerp(frame, [a.from + 6, a.from + 16], [1, 0]);
  const reopen = ease(frame, a.from + 40, 14);
  const callOpacity = frame < a.from + 40 ? closeT : reopen;
  // 2) Başka ekrana sürüklenir, geri gelir
  const dragOut = lerp(frame, [b.from + 2, b.from + 22], [0, 1]);
  const snapBack = ease(frame, b.from + 30, 16);
  const callX = 1 - 0.55 * dragOut * (1 - snapBack);
  const dragging = frame > b.from && frame < b.from + 30;
  // 3) Başka pencere üstüne gelmeye çalışır, çağrı ekranı üstte kalır
  const appX = lerp(frame, [c.from, c.from + 34], [0.15, 1.25]);

  const windows: Win[] = [
    ...(frame >= c.from ? [{ id: "app", kind: "app" as const, x: appX, y: 0.15, w: 0.6, h: 0.6, frame: true }] : []),
    { id: "call", kind: "call", x: callX, y: 0, w: 1, h: 1, opacity: callOpacity, scale: 0.94 + 0.06 * Math.max(callOpacity, 0) },
    ...(dragging ? [{ id: "cur", kind: "cursor" as const, x: callX + 0.5, y: 0.45, pressed: frame < b.from + 24 }] : []),
  ];

  const toast =
    frame > a.from + 40 && frame < b.from
      ? "Yeniden açıldı"
      : frame > b.from + 34 && frame < c.from
        ? "Geri getirildi"
        : frame >= c.from
          ? "En üstte"
          : null;

  return (
    <Backdrop>
      <Split
        header={<FeatureHeader index={3} title="Çağrı ekranı hep yerinde" size={sizes.header} />}
        first={
          <Card width={sizes.card} zoom={sizes.cardZoom}>
            {ITEMS.map(({ text, Icon }, i) => {
              const on = i <= active;
              const t = ease(frame, [a, b, c][i].from - 4, 12);
              return (
                <div
                  key={text}
                  style={{
                    display: "flex",
                    alignItems: "center",
                    gap: 20,
                    padding: "18px 20px",
                    marginBottom: i < 2 ? 16 : 0,
                    borderRadius: 18,
                    background: i === active ? "rgba(45,212,191,.12)" : "transparent",
                    border: `2px solid ${i === active ? colors.brandBright : colors.line}`,
                    opacity: on ? 0.55 + 0.45 * t : 0.35,
                  }}
                >
                  <div style={{ width: 56, height: 56, borderRadius: 14, background: on ? colors.brandBright : colors.line, display: "flex", alignItems: "center", justifyContent: "center" }}>
                    <Icon size={32} color={on ? colors.onBrand : colors.muted} strokeWidth={2.4} />
                  </div>
                  <div style={{ fontSize: 30, fontWeight: 700 }}>{text}</div>
                </div>
              );
            })}
          </Card>
        }
        second={
          <DualMonitors
            width={sizes.monitors}
            windows={windows}
            badge2={
              toast ? (
                <div style={{ background: colors.warning, color: colors.onBrand, fontWeight: 800, fontSize: 26, padding: "10px 18px", borderRadius: 12 }}>
                  {toast}
                </div>
              ) : null
            }
          />
        }
      />
    </Backdrop>
  );
};
