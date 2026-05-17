import { Toaster } from "react-hot-toast";

const AppToast = () => {
  return (
    <Toaster
      position="top-right"
      toastOptions={{
        duration: 3000,
        style: {
          fontSize: "14px",
        },
      }}
    />
  );
};

export default AppToast;