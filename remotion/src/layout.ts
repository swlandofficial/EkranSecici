import { useVideoConfig } from "remotion";

/** Yatay / dikey yerleşim ölçüleri. Altyazı alanı içerikten ayrılır. */
export const useLayout = () => {
  const { width, height } = useVideoConfig();
  const vertical = height > width;
  return {
    vertical,
    width,
    height,
    /** Altyazının alt kenara uzaklığı (dikeyde platform arayüzünün üstünde kalsın). */
    subtitleBottom: vertical ? 280 : 64,
    /** Sahne içeriğinin kullanabileceği alan */
    content: vertical
      ? { top: 150, bottom: height - 280 - 230, left: 60, right: width - 60 }
      : { top: 70, bottom: height - 200, left: 110, right: width - 110 },
  };
};
