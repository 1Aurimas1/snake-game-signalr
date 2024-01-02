import { UserDto } from "..";
import { GameMode } from "../../constants/GameMode";

export interface Point {
  X: number;
  Y: number;
}

export interface GridSize {
  rows: number;
  cols: number;
}

export interface GameObstacle {
  positions: Point[];
  points: Point[];
}

export interface GameStateDto {
  playerName: string;
  snakeParts: Point[];
  score: number;
  food?: Point;
}

export interface GameDto {
  id: number;
  name: string;
  isOpen: boolean;
  mode: GameMode;
  mapId: number;
  creator: UserDto;
  players: UserDto[];
}
