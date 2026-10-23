import React from "react";
import { colors } from "../theme";

type Props = {
  width: number;
  /** Ekranın içi; işlev verilirse ekranın iç genişlik/yüksekliğiyle çağrılır. */
  children?: React.ReactNode | ((screenW: number, screenH: number) => React.ReactNode);
  variant?: "staff" | "brand";
  label?: string;
  style?: React.CSSProperties;
};

export const monitorBezel = (width: number) => Math.max(6, width * 0.035);

/** İkondaki stile uygun basit monitör çizimi (ekran + ayak). */
export const Monitor: React.FC<Props> = ({ width, children, variant = "staff", label, style }) => {
  const body = variant === "brand" ? colors.monitorBodyBrand : colors.monitorBody;
  const screenH = (width * 9) / 16;
  const bezel = monitorBezel(width);
  const screenW = width - bezel * 2;
  return (
    <div style={{ display: "flex", flexDirection: "column", alignItems: "center", width, ...style }}>
      <div
        style={{
          width,
          height: screenH + bezel * 2,
          background: body,
          borderRadius: bezel * 1.6,
          padding: bezel,
          boxShadow: "0 20px 60px rgba(0,0,0,.45)",
        }}
      >
        <div
          style={{
            position: "relative",
            width: "100%",
            height: "100%",
            borderRadius: bezel * 0.6,
            overflow: "hidden",
            background: variant === "brand" ? "#0a2b28" : colors.monitorScreen,
          }}
        >
          {typeof children === "function" ? children(screenW, screenH) : children}
        </div>
      </div>
      <div style={{ width: width * 0.1, height: width * 0.07, background: body }} />
      <div style={{ width: width * 0.36, height: width * 0.03, background: body, borderRadius: width * 0.02 }} />
      {label ? (
        <div style={{ marginTop: width * 0.035, color: colors.muted, fontSize: Math.max(22, width * 0.05), fontWeight: 600 }}>
          {label}
        </div>
      ) : null}
    </div>
  );
};
