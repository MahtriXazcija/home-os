import { useQuery } from "@tanstack/react-query";
import { useHousehold } from "./useHousehold";
import { getApps } from "../api/apps";

export function useApps() {
  const { data: household } = useHousehold();
  const householdId = household?.id ?? "";
  const query = useQuery({
    queryKey: ["apps", householdId],
    queryFn: () => getApps(householdId),
    enabled: !!householdId,
  });
  return { ...query, householdId };
}
