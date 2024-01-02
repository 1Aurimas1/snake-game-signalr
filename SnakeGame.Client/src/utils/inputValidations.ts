import { GameMode } from "../shared/constants/GameMode";

const isValidEmail = (email: string): boolean => {
  if (email.includes(" ")) return false;

  const emailPattern =
    /(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|"(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9]))\.){3}(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9])|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])/;
  const re = RegExp(emailPattern);

  return re.test(email);
};

export const usernameValidation = {
  label: "username",
  id: "username",
  name: "username",
  type: "text",
  placeholder: "username...",
  validation: {
    required: {
      value: true,
      message: "This field can't be empty",
    },
    maxLength: {
      value: 20,
      message: "Should contain a maximum of 20 characters",
    },
  },
};

export const emailValidation = {
  label: "email address",
  id: "email",
  name: "email",
  type: "email",
  placeholder: "email...",
  validation: {
    required: {
      value: true,
      message: "This field can't be empty",
    },
    validate: (email: string) =>
      isValidEmail(email) || "Please enter a valid email.",
  },
};

export const passwordValidation = {
  label: "password",
  id: "password",
  name: "password",
  type: "password",
  placeholder: "password...",
  validation: {
    required: {
      value: true,
      message: "This field can't be empty",
    },
    minLength: {
      value: 6,
      message: "Should contain atleast 6 characters",
    },
  },
};

export const passwordConfirmationValidation = {
  label: "confirm password",
  id: "passwordConfirmation",
  name: "passwordConfirmation",
  type: "password",
  placeholder: "confirm password...",
  validation: {
    required: {
      value: true,
      message: "This field can't be empty",
    },
    minLength: {
      value: 6,
      message: "Should contain atleast 6 characters",
    },
    validate: {},
  },
};

export const mapNameValidation = {
  label: "name",
  id: "name",
  name: "name",
  type: "text",
  placeholder: "map name...",
  validation: {
    required: {
      value: true,
      message: "This field can't be empty",
    },
  },
};

export const mapRatingValidation = {
  label: "rating",
  id: "mapRating",
  name: "mapRating",
  type: "number",
  validation: {
    required: {
      value: true,
      message: "This field can't be empty",
    },
    min: {
      value: 1,
      message: "Minimum rating value should be at least 1",
    },
    max: {
      value: 5,
      message: "Maximum rating value should be below 6",
    },
    valueAsNumber: true,
  },
};

export const tournamentNameValidation = {
  label: "name",
  id: "name",
  name: "name",
  type: "text",
  placeholder: "tournament name...",
  validation: {
    required: {
      value: true,
      message: "This field can't be empty",
    },
  },
};

export const tournamentMaxParticipantsValidation = {
  label: "max participants",
  id: "maxparticipants",
  name: "maxparticipants",
  type: "number",
  validation: {
    required: {
      value: true,
      message: "This field can't be empty",
    },
    min: {
      value: 2,
      message: "Maximum participants should be at least 2",
    },
    valueAsNumber: true,
  },
};

export const startDateTimeValidation = {
  label: "start time",
  id: "startTime",
  name: "startTime",
  type: "date",
  placeholder: "start time...",
  validation: {
    required: {
      value: true,
      message: "This field can't be empty",
    },
  },
};

export const endDateTimeValidation = {
  label: "end time",
  id: "endTime",
  name: "endTime",
  type: "date",
  placeholder: "end time...",
  validation: {
    required: {
      value: true,
      message: "This field can't be empty",
    },
  },
};

export const tournamentRoundsValidation = {
  label: "rounds",
  id: "rounds",
  name: "rounds",
  type: "array",
};

export const tournamentRoundMapIdValidation = {
  label: "roundMapId",
  id: "roundMapId",
  name: "roundMapId",
  type: "number",
  validation: {
    required: {
      value: true,
      message: "This field can't be empty",
    },
    valueAsNumber: true,
  },
};

export const gameNameValidation = {
  label: "name",
  id: "name",
  name: "name",
  type: "text",
  placeholder: "game name...",
  validation: {
    required: {
      value: true,
      message: "This field can't be empty",
    },
  },
};

export const gameModeValidation = {
  label: "game mode",
  id: "gameMode",
  name: "mode",
  type: "text",
  validation: {
    required: {
      value: true,
      message: "This field can't be empty",
    },
    validate: (value) =>
      Object.values(GameMode).includes(parseInt(value) as GameMode) ||
      "Invalid game mode option",
    valueAsNumber: true,
  },
};

export const gameMapIdValidation = {
  label: "game map id",
  id: "gameMapId",
  name: "mapId",
  type: "number",
  validation: {
    required: {
      value: true,
      message: "This field can't be empty",
    },
    valueAsNumber: true,
  },
};

export const editUserNameValidation = {
  label: "username",
  id: "username",
  name: "username",
  type: "text",
  placeholder: "username...",
};

export const editEmailValidation = {
  label: "email address",
  id: "email",
  name: "email",
  type: "email",
  placeholder: "email...",
};
