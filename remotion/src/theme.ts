import { loadFont } from "@remotion/google-fonts/Inter";

// Ekran Seçici marka renkleri (docs/index.html ve uygulama ikonuyla aynı).
export const colors = {
  brand: "#0d9488",
  brandStrong: "#0f766e",
  brandBright: "#2dd4bf",
  brandSoft: "#ccfbf1",
  bg: "#0b1413",
  surface: "#111e1c",
  line: "#1f322f",
  text: "#e6f2f0",
  muted: "#9fb5b1",
  onBrand: "#04201d",
  // İkondaki monitör tonları
  monitorBody: "#374151",
  monitorScreen: "#94a3b8",
  monitorBodyBrand: "#115e59",
  danger: "#f87171",
  warning: "#fbbf24",
} as const;

// Uygulama Segoe UI kullanıyor; Google Fonts'taki en yakın karşılığı Inter.
export const { fontFamily } = loadFont("normal", {
  weights: ["400", "600", "700", "800", "900"],
  subsets: ["latin", "latin-ext"],
});

export const FPS = 30;
