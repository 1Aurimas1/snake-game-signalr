export function getFormattedDate(date: Date | string): string {
    const d = new Date(date);
    const options = {
      year: "numeric" as const,
      month: "numeric" as const,
      day: "numeric" as const,
      hour: "2-digit" as const,
      minute: "2-digit" as const,
    };
    const newDateFormat = d.toLocaleString("en-GB", options);

    return newDateFormat;
}
