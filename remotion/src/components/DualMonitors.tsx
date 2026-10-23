import React from "react";
import { CallScreen } from "./CallScreen";
import { Cursor } from "./Cursor";
import { Monitor } from "./Monitor";
import { colors } from "../theme";

/**
 * Sanal masaüstündeki pencere. Konum ve boyut ekran genişliği/yüksekliği cinsinden:
 * x ∈ [0,1] 1. ekran, x ∈ [1,2] 2. ekran. Böylece pencere iki ekran arasında kayabilir.
 */
export type Win = {
  id: string;
  kind: "call" | "app" | "browser" | "cursor";
  x: number;
  y: number;
  w?: number;
  h?: number;
  /** Başlık çubuğu (pencere çerçevesi) gösterilsin mi? Tam ekranda false. */
  frame?: boolean;
  opacity?: number;
  scale?: number;
  title?: string;
  pressed?: boolean;
};

type ScreenMode = "desktop" | "dark" | "boot";

type Props = {
  width: number;
  windows: Win[];
  /** Monitörleri alt alta diz (yalnızca tek panel olan dikey sahnelerde). */
  stack?: boolean;
  screen1?: ScreenMode;
  /** 2. ekran açık mı (0 = sinyal yok, 1 = açık) */
  screen2On?: number;
  labels?: [string, string];
  /** 2. ekranın sağ üstünde gösterilecek rozet */
  badge2?: React.ReactNode;
  staffApps?: boolean;
};

const TitleBar: React.FC<{ h: number; title: string; browser?: boolean }> = ({ h, title, browser }) => (
  <div
    style={{
      height: h,
      background: browser ? "#2b2a33" : "#e2e8f0",
      color: browser ? "#fbfbfe" : "#1e293b",
      display: "flex",
      alignItems: "center",
      gap: h * 0.3,
      padding: `0 ${h * 0.35}px`,
      fontSize: h * 0.5,
      fontWeight: 600,
      whiteSpace: "nowrap",
      overflow: "hidden",
      flexShrink: 0,
    }}
  >
    {browser ? (
      <div style={{ width: h * 0.5, height: h * 0.5, borderRadius: 999, background: "linear-gradient(135deg,#ff9500,#ff3750)" }} />
    ) : null}
    <span style={{ flex: 1, overflow: "hidden", textOverflow: "ellipsis" }}>{title}</span>
    <span style={{ letterSpacing: h * 0.25, opacity: 0.7 }}>— ▢ ✕</span>
  </div>
);

const AppBody: React.FC<{ w: number }> = ({ w }) => (
  <div style={{ flex: 1, background: "#f8fafc", padding: w * 0.04 }}>
    {[85, 70, 92, 55, 78].map((p, i) => (
      <div key={i} style={{ height: w * 0.025, width: `${p}%`, background: "#cbd5e1", borderRadius: 4, marginBottom: w * 0.03 }} />
    ))}
  </div>
);

const WindowView: React.FC<{ win: Win; sw: number; sh: number; screenIndex: number }> = ({ win, sw, sh, screenIndex }) => {
  if (win.kind === "cursor") {
    return <Cursor x={(win.x - screenIndex) * sw} y={win.y * sh} size={sw * 0.06} pressed={win.pressed} />;
  }
  const w = (win.w ?? 0.5) * sw;
  const h = (win.h ?? 0.5) * sh;
  const barH = win.frame ? Math.max(10, sh * 0.065) : 0;
  return (
    <div
      style={{
        position: "absolute",
        left: (win.x - screenIndex) * sw,
        top: win.y * sh,
        width: w,
        height: h,
        opacity: win.opacity ?? 1,
        transform: `scale(${win.scale ?? 1})`,
        borderRadius: win.frame ? sw * 0.008 : 0,
        overflow: "hidden",
        display: "flex",
        flexDirection: "column",
        boxShadow: win.frame ? "0 10px 30px rgba(0,0,0,.4)" : "none",
      }}
    >
      {win.frame ? (
        <TitleBar
          h={barH}
          browser={win.kind !== "app"}
          title={win.title ?? (win.kind === "call" ? "Hasta Çağrı — Mozilla Firefox" : win.kind === "browser" ? "Yeni Sekme — Mozilla Firefox" : "Hasta Kayıt")}
        />
      ) : null}
      {win.kind === "call" ? (
        <div style={{ position: "relative", flex: 1 }}>
          <CallScreen scale={w / 440} />
        </div>
      ) : win.kind === "browser" ? (
        <div style={{ flex: 1, background: "#f9f9fb", display: "flex", flexDirection: "column", alignItems: "center", paddingTop: h * 0.18 }}>
          <div style={{ width: "60%", height: h * 0.09, borderRadius: 999, background: "#e5e7eb" }} />
        </div>
      ) : (
        <AppBody w={w} />
      )}
    </div>
  );
};

const Screen: React.FC<{
  index: number;
  sw: number;
  sh: number;
  windows: Win[];
  mode: ScreenMode;
  on: number;
  staffApps: boolean;
}> = ({ index, sw, sh, windows, mode, on, staffApps }) => {
  const isStaff = index === 0;
  return (
    <div style={{ position: "absolute", inset: 0 }}>
      {/* Duvar kâğıdı */}
      <div
        style={{
          position: "absolute",
          inset: 0,
          background: isStaff
            ? "linear-gradient(135deg,#64748b,#94a3b8)"
            : "linear-gradient(135deg,#0b3b37,#0f2a28)",
        }}
      />
      {isStaff && staffApps ? (
        <>
          <WindowView win={{ id: "a1", kind: "app", x: 0.05, y: 0.08, w: 0.5, h: 0.55, frame: true, title: "Hasta Kayıt" }} sw={sw} sh={sh} screenIndex={0} />
          <WindowView win={{ id: "a2", kind: "app", x: 0.4, y: 0.3, w: 0.52, h: 0.55, frame: true, title: "Laboratuvar Sonuçları" }} sw={sw} sh={sh} screenIndex={0} />
        </>
      ) : null}
      {windows.filter((w) => w.kind !== "cursor").map((w) => (
        <WindowView key={w.id} win={w} sw={sw} sh={sh} screenIndex={index} />
      ))}
      {isStaff ? (
        <div style={{ position: "absolute", left: 0, right: 0, bottom: 0, height: sh * 0.08, background: "rgba(15,23,42,.7)" }} />
      ) : null}
      {windows.filter((w) => w.kind === "cursor").map((w) => (
        <WindowView key={w.id} win={w} sw={sw} sh={sh} screenIndex={index} />
      ))}
      {/* Kapalı ekran / açılış */}
      {mode !== "desktop" && isStaff ? (
        <div style={{ position: "absolute", inset: 0, background: "#05090a", display: "flex", alignItems: "center", justifyContent: "center" }}>
          {mode === "boot" ? <BootSpinner size={sh * 0.16} /> : null}
        </div>
      ) : null}
      {!isStaff && on < 1 ? (
        <div
          style={{
            position: "absolute",
            inset: 0,
            background: "#05090a",
            opacity: 1 - on,
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            color: "#475569",
            fontSize: sh * 0.09,
            fontWeight: 700,
          }}
        >
          Sinyal yok
        </div>
      ) : null}
    </div>
  );
};

const BootSpinner: React.FC<{ size: number }> = ({ size }) => (
  <div style={{ display: "flex", gap: size * 0.18 }}>
    {[0, 1, 2, 3, 4].map((i) => (
      <div key={i} style={{ width: size * 0.12, height: size * 0.12, borderRadius: 999, background: "#cbd5e1", opacity: 0.4 + i * 0.12 }} />
    ))}
  </div>
);

export const DualMonitors: React.FC<Props> = ({
  width,
  windows,
  stack = false,
  screen1 = "desktop",
  screen2On = 1,
  labels = ["1 · Personel", "2 · Bekleme salonu"],
  badge2,
  staffApps = true,
}) => {
  const m1 = stack ? width * 0.86 : width * 0.44;
  const m2 = stack ? width : width * 0.52;
  return (
    <div
      style={{
        display: "flex",
        flexDirection: stack ? "column" : "row",
        alignItems: stack ? "center" : "flex-end",
        justifyContent: "center",
        gap: stack ? width * 0.04 : width * 0.04,
        width,
      }}
    >
      <Monitor width={m1} label={labels[0]}>
        {(sw, sh) => <Screen index={0} sw={sw} sh={sh} windows={windows} mode={screen1} on={1} staffApps={staffApps} />}
      </Monitor>
      <div style={{ position: "relative" }}>
        <Monitor width={m2} variant="brand" label={labels[1]}>
          {(sw, sh) => <Screen index={1} sw={sw} sh={sh} windows={windows} mode="desktop" on={screen2On} staffApps={false} />}
        </Monitor>
        {badge2 ? <div style={{ position: "absolute", right: -m2 * 0.04, top: -m2 * 0.06 }}>{badge2}</div> : null}
      </div>
    </div>
  );
};
