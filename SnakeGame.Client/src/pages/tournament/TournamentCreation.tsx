import { useState } from "react";
import { Controller, useFieldArray, useForm } from "react-hook-form";
import {
  tournamentNameValidation,
  tournamentMaxParticipantsValidation,
  startDateTimeValidation,
  endDateTimeValidation,
  tournamentRoundsValidation,
} from "../../utils/inputValidations";
import { Dayjs } from "dayjs";
import { useAuth } from "../../hooks/useAuth";
import { ApiErrorResponse, ErrorOrSuccess } from "../../shared/interfaces";
import { Button, Form, Input } from "../../components";

interface CreateRoundDto {
  index: number;
  mapId: number;
}

interface CreateTournamentDto {
  name: string;
  startTime: Dayjs;
  endTime: Dayjs;
  MaxParticipants: number;
  rounds: CreateRoundDto[];
}

export const TournamentCreation = () => {
  const { authFetch } = useAuth();
  const [apiError, setApiError] = useState<ApiErrorResponse | null>(null);
  const [errorOrSuccess, setErrorOrSuccess] = useState<ErrorOrSuccess | null>(
    null,
  );

  const methods = useForm<CreateTournamentDto>();

  const { fields, append, remove } = useFieldArray({
    control: methods.control,
    name: "rounds",
  });

  function addNewRound() {
    methods.clearErrors([tournamentRoundsValidation.name as "rounds"]);
    append({ index: fields.length + 1, mapId: 1 });
  }

  async function onSubmit(createTournamentDto: CreateTournamentDto) {
    const { apiError } = await authFetch(
      "/tournaments",
      { method: "POST" },
      createTournamentDto,
    );

    if (apiError) {
      setApiError(apiError);
    } else {
      setApiError(null);
      methods.reset({ rounds: [] });
      setErrorOrSuccess({ success: "Tournament created successfully" });
    }
  }

  return (
    <div className="flex flex-col items-center justify-center gap-5">
      <h1 className="mb-7 text-2xl font-semibold">Tournament creator</h1>
      <Form
        methods={methods}
        onSubmit={onSubmit}
        errorOrSuccess={errorOrSuccess}
      >
        <Input {...tournamentNameValidation} apiErrorResponse={apiError} />
        <Input
          {...tournamentMaxParticipantsValidation}
          apiErrorResponse={apiError}
        />
        <Input {...startDateTimeValidation} apiErrorResponse={apiError} />
        <Input {...endDateTimeValidation} apiErrorResponse={apiError} />

        <Input {...tournamentRoundsValidation} apiErrorResponse={apiError}>
          <div className="flex flex-row justify-start gap-2">
            <label className="w-16 capitalize">nr.</label>
            <label className="w-16 capitalize">map id</label>
          </div>
          <ol className="flex flex-col items-start justify-center gap-2">
            {fields.map((item, index) => (
              <li
                key={item.id}
                className="flex flex-row items-center justify-center gap-2"
              >
                <input
                  readOnly
                  className="w-16 rounded-md border-2 bg-gray-100 p-3 font-medium placeholder:opacity-50"
                  {...methods.register(`rounds.${index}.index`)}
                />
                <Controller
                  render={({ field }) => (
                    <input
                      className="w-16 rounded-md border-2 p-3 font-medium placeholder:opacity-50"
                      {...field}
                    />
                  )}
                  name={`rounds.${index}.mapId`}
                  control={methods.control}
                />
                {index === fields.length - 1 && (
                  <Button
                    type="button"
                    className="w-12"
                    onClick={() => remove(index)}
                  >
                    Del
                  </Button>
                )}
              </li>
            ))}
          </ol>
          <Button
            className="mt-2 w-12"
            type="button"
            onClick={() => addNewRound()}
          >
            Add
          </Button>
        </Input>

        <Button type="submit">Create</Button>
      </Form>
    </div>
  );
};

export default TournamentCreation;
