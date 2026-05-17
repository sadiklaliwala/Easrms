// import AppRoutes from "./routes/AppRoutes";

// function App() {
//   return (
//     <>
//       <AppRoutes />
//     </>
//   );
// }

// export default App;

import { BrowserRouter } from "react-router-dom";
import AppRoutes from "./routes/AppRoutes";
import AppToast from "./components/common/feedback/AppToast";

const App = () => {
  return (
    <BrowserRouter>
      <AppToast />
      <AppRoutes />
    </BrowserRouter>
  );
};

export default App;
