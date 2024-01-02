export const CellType = {
  EMPTY: 0,
  SNAKE_HEAD: 1,
  SNAKE_BODY: 2,
  FOOD: 3,
  OBSTACLE: 4,
};
export type CellType = (typeof CellType)[keyof typeof CellType];
