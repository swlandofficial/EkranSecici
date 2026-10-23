import React from "react";
import { AbsoluteFill, Audio, Sequence, staticFile } from "remotion";
import { linearTiming, TransitionSeries } from "@remotion/transitions";
import { fade } from "@remotion/transitions/fade";
import { slide } from "@remotion/transitions/slide";
import { Subtitles } from "./components/Subtitles";
import { Autostart } from "./scenes/Autostart";
import { Firefox } from "./scenes/Firefox";
import { Intro } from "./scenes/Intro";
import { Monitors } from "./scenes/Monitors";
import { NoAdmin } from "./scenes/NoAdmin";
import { Outro } from "./scenes/Outro";
import { Reveal } from "./scenes/Reveal";
import { Watchdog } from "./scenes/Watchdog";
import { LEAD, SceneProps, SectionId, sceneFrames, sceneStarts, Timings, toSceneSentences, TRANSITION_FRAMES } from "./timeline";
import { FPS } from "./theme";

export type VideoProps = { timings: Timings; hasAudio: boolean };

const SCENES: Record<SectionId, React.FC<SceneProps>> = {
  intro: Intro,
  reveal: Reveal,
  monitors: Monitors,
  autostart: Autostart,
  watchdog: Watchdog,
  firefox: Firefox,
  noadmin: NoAdmin,
  outro: Outro,
};

export const Video: React.FC<VideoProps> = ({ timings, hasAudio }) => {
  const sections = timings.sections;
  const { starts } = sceneStarts(sections);

  // Altyazılar ve ses geçişlerden bağımsız, videonun mutlak zamanına göre yerleştirilir.
  const globalSentences = sections.flatMap((s, i) =>
    toSceneSentences(s).map((x) => ({ ...x, from: x.from + starts[i], to: x.to + starts[i] })),
  );

  return (
    <AbsoluteFill style={{ background: "#0b1413" }}>
      <TransitionSeries>
        {sections.map((section, i) => {
          const Scene = SCENES[section.id];
          const duration = sceneFrames(section);
          return (
            <React.Fragment key={section.id}>
              <TransitionSeries.Sequence durationInFrames={duration}>
                <Scene sentences={toSceneSentences(section)} duration={duration} />
              </TransitionSeries.Sequence>
              {i < sections.length - 1 ? (
                <TransitionSeries.Transition
                  presentation={i === 0 || i === sections.length - 2 ? fade() : slide({ direction: "from-right" })}
                  timing={linearTiming({ durationInFrames: TRANSITION_FRAMES })}
                />
              ) : null}
            </React.Fragment>
          );
        })}
      </TransitionSeries>

      {hasAudio
        ? sections.map((section, i) => (
            <Sequence key={section.id} from={starts[i] + Math.round(LEAD * FPS)} layout="none">
              <Audio src={staticFile(section.file)} />
            </Sequence>
          ))
        : null}

      <Subtitles sentences={globalSentences} />
    </AbsoluteFill>
  );
};
