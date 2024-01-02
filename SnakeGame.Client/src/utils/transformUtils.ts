import {
  GameObstacle,
  ObstacleDto,
  Point,
  PointDto,
} from "../shared/interfaces";

export function transformToPoint(p: PointDto): Point {
  return {
    X: p.X,
    Y: p.Y,
  };
}

export function transformToGameObstacle(
  o: ObstacleDto,
  positions: PointDto[] | Point,
): GameObstacle {
  return {
    positions: Array.isArray(positions) ? positions : [positions],
    points: o.points.map((p) => transformToPoint(p)),
  };
}
