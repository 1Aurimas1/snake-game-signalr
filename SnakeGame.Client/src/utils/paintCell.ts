import { CellType } from "../shared/constants/CellType";

export function paintCell(cell: CellType): string {
  switch (cell) {
    case CellType.SNAKE_BODY:
      return "bg-gray-500";
    case CellType.SNAKE_HEAD:
      return "bg-black";
    case CellType.FOOD:
      return "bg-red-500";
    case CellType.OBSTACLE:
      return "bg-gray-700";
    default:
      return "bg-gray-200";
  }
}
