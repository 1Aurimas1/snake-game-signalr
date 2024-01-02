import {
  Dialog,
  DialogHeader,
  DialogBody,
  DialogFooter,
} from "@material-tailwind/react";
import { ReactNode } from "react";
import { Button } from "..";

interface Props {
  open: boolean;
  handleOpen: () => void;
  header: string;
  children: ReactNode;
  onConfirm: (arg: any) => Promise<void>;
  confirmButtonCN?: string;
}

export const Modal = (props: Props) => {
  return (
    <Dialog
      open={props.open}
      handler={props.handleOpen}
      className="fixed left-1/4 top-1/3 w-1/2 min-w-fit border-2 border-black bg-gray-50"
    >
      <DialogHeader>{props.header}</DialogHeader>
      <DialogBody>{props.children}</DialogBody>
      <DialogFooter className="flex justify-end">
        <Button
          type="button"
          onClick={props.handleOpen}
          className="mr-2 bg-zinc-300"
        >
          Cancel
        </Button>
        <Button
          type="button"
          onClick={props.onConfirm}
          className={props.confirmButtonCN}
        >
          Confirm
        </Button>
      </DialogFooter>
    </Dialog>
  );
};

export default Modal;
