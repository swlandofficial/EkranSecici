import React from "react";
import { AbsoluteFill } from "remotion";
import { colors, fontFamily } from "../theme";

/** Tüm sahnelerin ortak arka planı. */
export const Backdrop: React.FC<{ children?: React.ReactNode; glow?: string }> = ({ children, glow = "70% 40%" }) => (
  <AbsoluteFill
    style={{
      fontFamily,
      color: colors.text,
      background: `radial-gradient(circle at ${glow}, rgba(45,212,191,.16), transparent 55%), ${colors.bg}`,
    }}
  >
    <AbsoluteFill
      style={{
        backgroundImage:
          "linear-gradient(rgba(255,255,255,.025) 1px, transparent 1px), linear-gradient(90deg, rgba(255,255,255,.025) 1px, transparent 1px)",
        backgroundSize: "64px 64px",
      }}
    />
    {children}
  </AbsoluteFill>
);

/** Özellik başlığı: numara rozeti + başlık. */
export const FeatureHeader: React.FC<{ index: number; title: string; size?: number; style?: React.CSSProperties }> = ({
  index,
  title,
  size = 56,
  style,
}) => (
  <div style={{ display: "flex", alignItems: "center", gap: size * 0.35, ...style }}>
    <div
      style={{
        width: size * 1.15,
        height: size * 1.15,
        borderRadius: size * 0.3,
        background: colors.brandBright,
        color: colors.onBrand,
        fontWeight: 900,
        fontSize: size * 0.7,
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        flexShrink: 0,
      }}
    >
      {index}
    </div>
    <div style={{ fontSize: size, fontWeight: 800, letterSpacing: -1, lineHeight: 1.1 }}>{title}</div>
  </div>
);

export const Card: React.FC<{ children: React.ReactNode; width: number; zoom?: number; style?: React.CSSProperties }> = ({
  children,
  width,
  zoom = 1,
  style,
}) => (
  <div
    style={{
      width,
      zoom,
      background: colors.surface,
      border: `2px solid ${colors.line}`,
      borderRadius: 28,
      padding: 36,
      boxShadow: "0 30px 80px rgba(0,0,0,.35)",
      ...style,
    }}
  >
    {children}
  </div>
);

export const Chip: React.FC<{ children: React.ReactNode; active?: boolean; size?: number; style?: React.CSSProperties }> = ({
  children,
  active,
  size = 26,
  style,
}) => (
  <div
    style={{
      fontSize: size,
      fontWeight: 700,
      padding: `${size * 0.4}px ${size * 0.7}px`,
      borderRadius: 999,
      background: active ? colors.brandBright : "transparent",
      color: active ? colors.onBrand : colors.muted,
      border: `2px solid ${active ? colors.brandBright : colors.line}`,
      whiteSpace: "nowrap",
      ...style,
    }}
  >
    {children}
  </div>
);

/** Form satırı: etiket + değer kutusu */
export const Field: React.FC<{ label: string; children: React.ReactNode; highlight?: number; size?: number }> = ({
  label,
  children,
  highlight = 0,
  size = 26,
}) => (
  <div style={{ marginBottom: size * 0.9 }}>
    <div style={{ fontSize: size * 0.8, color: colors.muted, fontWeight: 600, marginBottom: size * 0.3 }}>{label}</div>
    <div
      style={{
        fontSize: size,
        fontWeight: 600,
        padding: `${size * 0.45}px ${size * 0.6}px`,
        borderRadius: 14,
        background: colors.bg,
        border: `2px solid ${colors.line}`,
        boxShadow: highlight > 0 ? `0 0 0 ${4 * highlight}px rgba(45,212,191,${0.5 * highlight})` : "none",
        display: "flex",
        alignItems: "center",
        gap: 12,
        minHeight: size * 1.5,
        whiteSpace: "nowrap",
        overflow: "hidden",
      }}
    >
      {children}
    </div>
  </div>
);
