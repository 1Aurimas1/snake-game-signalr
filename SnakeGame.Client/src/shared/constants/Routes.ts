export const LOGIN = "/login";
export const REGISTER = "/register";
export const LOGOUT = "/logout";
export const CATCH_ALL = "/*";

export const HOME = "/";
export const PLAY = "/play";
export const PROFILE = "/profile";

export const USERS = "/users";
export const USER_DETAILS = `${USERS}/:userId`;

export const MAPS = "/maps";
export const MAP_CREATION = `${MAPS}/new`;
export const MAP_DETAILS = `${USERS}/:userId${MAPS}/:mapId`;

export const GAMES = "/games";
export const GAME_CREATION = `${GAMES}/new`;
export const GAME_DETAILS = `${USERS}/:userId${GAMES}/:gameId`;

export const TOURNAMENTS = "/tournaments";
export const TOURNAMENT_CREATION = `${TOURNAMENTS}/new`;
export const TOURNAMENT_DETAILS = `${USERS}/:userId${TOURNAMENTS}/:tournamentId`;
