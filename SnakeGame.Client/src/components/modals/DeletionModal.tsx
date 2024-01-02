import Modal from "./Modal";

interface Props {
  open: boolean;
  handleOpen: () => void;
  onSubmit: (arg: any) => Promise<void>;
}

const DeletionModal = (props: Props) => {
  return (
    <Modal
      open={props.open}
      handleOpen={props.handleOpen}
      header="Delete Confirmation"
      onConfirm={props.onSubmit}
      confirmButtonCN="bg-red-500"
    >
      <p>Are you sure you want to delete this item?</p>
    </Modal>
  );
};

export default DeletionModal;
