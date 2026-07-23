import { Navigate, Outlet } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { getMyHousehold } from "../api/household";

export default function RequireHousehold() {
  const { data, isLoading } = useQuery({ queryKey: ["my-household"], queryFn: getMyHousehold });

  if (isLoading) return null;
  if (!data) return <Navigate to="/onboarding" replace />;

  return <Outlet />;
}
