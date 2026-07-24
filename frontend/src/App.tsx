import { Routes, Route } from "react-router-dom";
import Layout from "./components/Layout";
import RequireAuth from "./components/RequireAuth";
import RequireHousehold from "./components/RequireHousehold";
import Dashboard from "./pages/Dashboard";
import Login from "./pages/Login";
import Register from "./pages/Register";
import Onboarding from "./pages/Onboarding";
import Tasks from "./pages/Tasks";
import Kanban from "./pages/Kanban";
import CalendarPage from "./pages/Calendar";
import Reminders from "./pages/Reminders";
import Notes from "./pages/Notes";
import Finance from "./pages/Finance";
import LifeAdmin from "./pages/LifeAdmin";
import ManageApps from "./pages/ManageApps";
import MealPlanner from "./pages/MealPlanner";
import Chat from "./pages/Chat";
import Profile from "./pages/Profile";

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
            <Route path="tasks" element={<Tasks />} />
            <Route path="kanban" element={<Kanban />} />
            <Route path="calendar" element={<CalendarPage />} />
            <Route path="reminders" element={<Reminders />} />
            <Route path="notes" element={<Notes />} />
            <Route path="finance" element={<Finance />} />
            <Route path="life-admin" element={<LifeAdmin />} />
            <Route path="apps" element={<ManageApps />} />
            <Route path="meal-planner" element={<MealPlanner />} />
            <Route path="chat" element={<Chat />} />
            <Route path="profile" element={<Profile />} />
          </Route>
        </Route>
      </Route>
    </Routes>
  );
}
