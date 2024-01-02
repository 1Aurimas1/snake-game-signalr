import { GameMode } from "../shared/constants/GameMode";

export default function getGameModeKey(value: number): keyof typeof GameMode {
  let mode: GameMode = value as GameMode;
  let modeKey: keyof typeof GameMode = Object.keys(GameMode)[
    mode
  ] as keyof typeof GameMode;
  return modeKey;
}
