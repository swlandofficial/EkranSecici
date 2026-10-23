import React from "react";
import { colors } from "../theme";

/** Personel monitöründeki "dolu masaüstü": birkaç uygulama penceresi. */
export const StaffDesktop: React.FC<{ scale?: number }> = ({ scale = 1 }) => {
  const win = (left: string, top: string, w: string, h: string, opacity = 0.95) => (
    <div
      style={{
        position: "absolute",
        left,
        top,
        width: w,
        height: h,
        background: "#f8fafc",
        opacity,
        borderRadius: 6 * scale,
        overflow: "hidden",
        boxShadow: "0 6px 18px rgba(0,0,0,.25)",
      }}
    >
      <div style={{ height: 16 * scale, background: colors.monitorBody, opacity: 0.7 }} />
      {[0, 1, 2].map((i) => (
        <div
          key={i}
          style={{
            margin: `${10 * scale}px ${12 * scale}px 0`,
            height: 8 * scale,
            width: `${80 - i * 20}%`,
            background: "#cbd5e1",
            borderRadius: 4 * scale,
          }}
        />
      ))}
    </div>
  );
  return (
    <div style={{ position: "absolute", inset: 0 }}>
      {win("6%", "8%", "55%", "55%")}
      {win("40%", "35%", "52%", "52%", 0.9)}
      <div style={{ position: "absolute", left: 0, right: 0, bottom: 0, height: "9%", background: "rgba(15,23,42,.55)" }} />
    </div>
  );
};
