import { Routes, Route } from "react-router-dom";
import Layout from "./components/Layout";
import RequireAuth from "./components/RequireAuth";
import RequireHousehold from "./components/RequireHousehold";
import Dashboard from "./pages/Dashboard";
import Placeholder from "./pages/Placeholder";
import Login from "./pages/Login";
import Register from "./pages/Register";
import Onboarding from "./pages/Onboarding";

export default function App() {
  return (
    <Routes>
      <Route path="login" element={<Login />} />
      <Route path="register" element={<Register />} />

      <Route element={<RequireAuth />}>
        <Route path="onboarding" element={<Onboarding />} />

        <Route element={<RequireHousehold />}>
          <Route element={<Layout />}>
            <Route index element={<Dashboard />} />
            <Route path="tasks" element={<Placeholder title="Tasks" phase="Phase 2" />} />
            <Route path="kanban" element={<Placeholder title="Kanban" phase="Phase 2" />} />
            <Route path="calendar" element={<Placeholder title="Calendar" phase="Phase 2" />} />
            <Route path="reminders" element={<Placeholder title="Reminders" phase="Phase 3" />} />
            <Route path="notes" element={<Placeholder title="Notes" phase="Phase 4" />} />
            <Route path="finance" element={<Placeholder title="Finance" phase="Phase 4" />} />
            <Route path="life-admin" element={<Placeholder title="Life Admin" phase="Phase 4" />} />
          </Route>
        </Route>
      </Route>
    </Routes>
  );
}
