import LoginPage from './Components/Login';
import Register from './Components/Register';
import 'process/browser'; // or require('process/browser');

import {
  BrowserRouter as Router,
  Routes,
  Route,
  Navigate,
} from "react-router-dom";

function App() {
  return (
    <>
        <Router>
            <Routes>
                <Route   path="/" element={<LoginPage />} />
                <Route  path="/Register" element={<Register />} />
            </Routes>
        </Router>
    </>
  );
}

export default App;
