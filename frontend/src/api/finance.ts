import { apiDelete, apiGet, apiPost, apiPut } from "./client";

export type TransactionType = "Income" | "Expense";
export type BillRecurrence = "None" | "Monthly" | "Yearly";

export interface Transaction {
  id: string;
  householdId: string;
  type: TransactionType;
  category: string;
  amount: number;
  occurredAtUtc: string;
  description: string | null;
  createdByUserId: string;
}

export interface Bill {
  id: string;
  householdId: string;
  title: string;
  amount: number;
  category: string;
  dueDateUtc: string;
  recurrence: BillRecurrence;
  isPaid: boolean;
  paidByUserId: string | null;
  paidAtUtc: string | null;
}

export interface Budget {
  id: string;
  category: string;
  monthlyLimit: number;
  spentThisMonth: number;
}

export interface MemberSpend {
  userId: string;
  paidThisMonth: number;
}

export interface FinanceSummary {
  totalIncomeThisMonth: number;
  totalExpenseThisMonth: number;
  spendByMember: MemberSpend[];
  budgets: Budget[];
}

export const getTransactions = (householdId: string) => apiGet<Transaction[]>(`/api/finance/transactions?householdId=${householdId}`);
export const createTransaction = (input: { householdId: string; type: TransactionType; category: string; amount: number; occurredAtUtc: string; description?: string }) =>
  apiPost<Transaction>("/api/finance/transactions", input);
export const deleteTransaction = (id: string) => apiDelete(`/api/finance/transactions/${id}`);

export const getBills = (householdId: string) => apiGet<Bill[]>(`/api/finance/bills?householdId=${householdId}`);
export const createBill = (input: { householdId: string; title: string; amount: number; category: string; dueDateUtc: string; recurrence: BillRecurrence }) =>
  apiPost<Bill>("/api/finance/bills", input);
export const payBill = (id: string) => apiPost<Bill>(`/api/finance/bills/${id}/pay`);

export const setBudget = (householdId: string, category: string, monthlyLimit: number) =>
  apiPut<void>("/api/finance/budgets", { householdId, category, monthlyLimit });

export const getFinanceSummary = (householdId: string) => apiGet<FinanceSummary>(`/api/finance/summary?householdId=${householdId}`);
