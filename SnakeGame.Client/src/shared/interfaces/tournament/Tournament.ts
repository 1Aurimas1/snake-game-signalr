import { UserDto } from "..";

export interface RoundDto {
    id: number;
    index: number; mapId: number;
}

export interface TournamentDto {
  id: number;
  name: string;
  startTime: Date;
  endTime: Date;
  currentRound: number;
  maxParticipants: number;
  rounds: RoundDto[];
  participants: UserDto[];
  organizer: UserDto;
}
