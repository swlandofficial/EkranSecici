import React from "react";
import { CalculateMetadataFunction, Composition, staticFile, Still } from "remotion";
import { Thumbnail } from "./Thumbnail";
import { estimatedTimings, sceneStarts, Timings } from "./timeline";
import { FPS } from "./theme";
import { Video, VideoProps } from "./Video";

/**
 * Süre, seslendirme betiğinin ürettiği public/voice/timings.json'dan hesaplanır.
 * Dosya yoksa (npm run voice çalıştırılmadıysa) metin uzunluğundan tahmin edilir ve ses çalınmaz.
 */
const calculateMetadata: CalculateMetadataFunction<VideoProps> = async () => {
  let timings: Timings = estimatedTimings();
  let hasAudio = false;
  try {
    const res = await fetch(staticFile("voice/timings.json"));
    if (res.ok) {
      timings = (await res.json()) as Timings;
      hasAudio = true;
    }
  } catch {
    // seslendirme yok: tahmini süreler
  }
  return { durationInFrames: sceneStarts(timings.sections).total, props: { timings, hasAudio } };
};

const defaultProps: VideoProps = { timings: estimatedTimings(), hasAudio: false };

export const RemotionRoot: React.FC = () => {
  return (
    <>
      <Composition
        id="EkranSeciciYatay"
        component={Video}
        width={1920}
        height={1080}
        fps={FPS}
        durationInFrames={sceneStarts(defaultProps.timings.sections).total}
        defaultProps={defaultProps}
        calculateMetadata={calculateMetadata}
      />
      <Composition
        id="EkranSeciciDikey"
        component={Video}
        width={1080}
        height={1920}
        fps={FPS}
        durationInFrames={sceneStarts(defaultProps.timings.sections).total}
        defaultProps={defaultProps}
        calculateMetadata={calculateMetadata}
      />
      <Still id="YoutubeKapak" component={Thumbnail} width={1280} height={720} />
    </>
  );
};
