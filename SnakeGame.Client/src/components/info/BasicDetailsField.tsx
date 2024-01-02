import { getFormattedDate } from "../../utils/getFormattedDate";

interface Props {
  fieldName: string;
  fieldValue: any;
}

function isValidDate(date: string): boolean {
  const dateObj = new Date(date);
  return !isNaN(dateObj.getTime());
}

function renderFieldValue(fieldValue: any): React.ReactNode {
  switch (typeof fieldValue) {
    case "object":
      if (Array.isArray(fieldValue)) {
        return fieldValue.length > 0 ? (
          fieldValue.map((value, idx) => {
            let separator = idx !== fieldValue.length - 1 ? ", " : "";
            return (
              <span key={idx}>
                {value}
                {separator}
              </span>
            );
          })
        ) : (
          <span>[Is empty]</span>
        );
      }
      break;
    case "boolean":
      return <span className="text-gray-500">{fieldValue ? "✔" : "✖"}</span>;
    case "string":
      if (isValidDate(fieldValue)) {
        return <span>{getFormattedDate(fieldValue)}</span>;
      }
    default:
      return <span>{fieldValue}</span>;
  }
}

const BasicDetailsField = (props: Props) => {
  const render = renderFieldValue(props.fieldValue);

  return (
    <p className="border-b border-gray-400 p-[1px]">
      <span className="font-bold">{props.fieldName}: </span>
      {render}
    </p>
  );
};

export default BasicDetailsField;
