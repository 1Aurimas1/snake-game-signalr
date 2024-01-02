import { UserDto } from "..";

export interface PointDto {
  id: number;
  X: number;
  Y: number;
}

export interface ObstacleDto {
  id: number;
  points: PointDto[];
}

export interface MapObstacleDto {
  mapObstacleId: number;
  position: PointDto;
  obstacleId: number;
}

export interface MapDto {
  id: number;
  name: string;
  rating: number;
  mapObstacles: MapObstacleDto[];
  isPublished: boolean;
  creator: UserDto;
}
