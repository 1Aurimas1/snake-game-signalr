import { useState } from "react";
import { useForm } from "react-hook-form";
import {
  gameNameValidation,
  gameModeValidation,
  gameMapIdValidation,
} from "../../utils/inputValidations";
import { useAuth } from "../../hooks/useAuth";
import { GameMode } from "../../shared/constants/GameMode";
import { ApiErrorResponse, ErrorOrSuccess } from "../../shared/interfaces";
import { Button, Form, Input } from "../../components";

interface CreateGameDto {
  name: string;
  mode: GameMode;
  mapId: number;
}

export const GameCreation = () => {
  const { authFetch } = useAuth();
  const [apiError, setApiError] = useState<ApiErrorResponse | null>(null);
  const [errorOrSuccess, setErrorOrSuccess] = useState<ErrorOrSuccess | null>(
    null,
  );

  const methods = useForm<CreateGameDto>();

  async function onSubmit(createGameDto: CreateGameDto) {
    const { apiError } = await authFetch(
      "/games",
      { method: "POST" },
      createGameDto,
    );

    if (apiError) {
      setApiError(apiError);
    } else {
      setApiError(null);
      methods.reset();
      setErrorOrSuccess({ success: "Game created successfully" });
    }
  }

  return (
    <div className="flex flex-col items-center justify-center gap-5">
      <h1 className="mb-7 text-2xl font-semibold">Game creator</h1>
      <Form
        methods={methods}
        onSubmit={onSubmit}
        errorOrSuccess={errorOrSuccess}
      >
        <Input {...gameNameValidation} apiErrorResponse={apiError} />
        <Input {...gameModeValidation} apiErrorResponse={apiError}>
          <select
            {...methods.register(
              gameModeValidation.name as "mode",
              gameModeValidation.validation,
            )}
            className="w-32 rounded-md border-2 p-3 font-medium"
          >
            {Object.keys(GameMode).map((m, idx) => (
              <option
                key={idx}
                value={GameMode[m as keyof typeof GameMode]}
                className="font-medium"
              >
                {m}
              </option>
            ))}
          </select>
        </Input>

        <Input {...gameMapIdValidation} apiErrorResponse={apiError} />

        <Button type="submit">Create</Button>
      </Form>
    </div>
  );
};

export default GameCreation;
