export const Direction = {
  UP: 0,
  DOWN: 1,
  LEFT: 2,
  RIGHT: 3,
} as const;

export type Direction = (typeof Direction)[keyof typeof Direction];
