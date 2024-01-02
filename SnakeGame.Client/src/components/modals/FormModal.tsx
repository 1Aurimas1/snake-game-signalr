import { ReactNode } from "react";
import { UseFormReturn } from "react-hook-form";
import Modal from "./Modal";
import { ErrorOrSuccess } from "../../shared/interfaces";
import { Form } from "..";

interface Props {
  open: boolean;
  handleOpen: () => void;
  header: string;
  methods: UseFormReturn<any, any, undefined>;
  onSubmit: (arg: any) => Promise<void>;
  errorOrSuccess: ErrorOrSuccess | null;
  children: ReactNode;
}

const FormModal = (props: Props) => {
  return (
    <Modal
      open={props.open}
      handleOpen={props.handleOpen}
      header={props.header}
      onConfirm={props.methods.handleSubmit(props.onSubmit)}
    >
      <Form
        methods={props.methods}
        onSubmit={props.onSubmit}
        errorOrSuccess={props.errorOrSuccess}
      >
        {props.children}
      </Form>
    </Modal>
  );
};

export default FormModal;
