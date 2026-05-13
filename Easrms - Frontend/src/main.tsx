import { createRoot } from "react-dom/client";
import { LocalizationProvider } from "@mui/x-date-pickers";
import { AdapterDateFns } from "@mui/x-date-pickers/AdapterDateFns";
import App from "./App";
import { Provider } from "react-redux";
import { store } from "./store";
import { ThemeProvider } from "@mui/material/styles";
import theme from "./theme/theme";

createRoot(document.getElementById("root")!).render(
  <LocalizationProvider dateAdapter={AdapterDateFns}>
    <Provider store={store}>
      <ThemeProvider theme={theme}>
        
        <App />
      </ThemeProvider>
    </Provider>
  </LocalizationProvider>,
);
