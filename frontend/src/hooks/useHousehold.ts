import { useQuery } from "@tanstack/react-query";
import { getMyHousehold } from "../api/household";

export function useHousehold() {
  return useQuery({ queryKey: ["my-household"], queryFn: getMyHousehold });
}
