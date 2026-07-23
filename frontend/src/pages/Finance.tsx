import { useState, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useHousehold } from "../hooks/useHousehold";
import {
  createBill,
  createTransaction,
  deleteTransaction,
  getBills,
  getFinanceSummary,
  getTransactions,
  payBill,
  setBudget,
  type BillRecurrence,
  type TransactionType,
} from "../api/finance";

function money(n: number) {
  return n.toLocaleString(undefined, { style: "currency", currency: "USD" });
}

export default function Finance() {
  const { data: household } = useHousehold();
  const householdId = household?.id ?? "";
  const queryClient = useQueryClient();

  const { data: summary } = useQuery({ queryKey: ["finance-summary", householdId], queryFn: () => getFinanceSummary(householdId), enabled: !!householdId });
  const { data: transactions } = useQuery({ queryKey: ["transactions", householdId], queryFn: () => getTransactions(householdId), enabled: !!householdId });
  const { data: bills } = useQuery({ queryKey: ["bills", householdId], queryFn: () => getBills(householdId), enabled: !!householdId });

  const invalidateAll = () => {
    queryClient.invalidateQueries({ queryKey: ["finance-summary", householdId] });
    queryClient.invalidateQueries({ queryKey: ["transactions", householdId] });
    queryClient.invalidateQueries({ queryKey: ["bills", householdId] });
  };

  // transaction form
  const [txType, setTxType] = useState<TransactionType>("Expense");
  const [txCategory, setTxCategory] = useState("");
  const [txAmount, setTxAmount] = useState("");
  const createTxMutation = useMutation({
    mutationFn: createTransaction,
    onSuccess: () => { setTxCategory(""); setTxAmount(""); invalidateAll(); },
  });
  function handleAddTransaction(e: FormEvent) {
    e.preventDefault();
    const amount = parseFloat(txAmount);
    if (!txCategory.trim() || !amount || !householdId) return;
    createTxMutation.mutate({ householdId, type: txType, category: txCategory, amount, occurredAtUtc: new Date().toISOString() });
  }
  const deleteTxMutation = useMutation({ mutationFn: deleteTransaction, onSuccess: invalidateAll });

  // bill form
  const [billTitle, setBillTitle] = useState("");
  const [billAmount, setBillAmount] = useState("");
  const [billCategory, setBillCategory] = useState("");
  const [billDue, setBillDue] = useState("");
  const [billRecurrence, setBillRecurrence] = useState<BillRecurrence>("Monthly");
  const createBillMutation = useMutation({
    mutationFn: createBill,
    onSuccess: () => { setBillTitle(""); setBillAmount(""); setBillCategory(""); setBillDue(""); invalidateAll(); },
  });
  function handleAddBill(e: FormEvent) {
    e.preventDefault();
    const amount = parseFloat(billAmount);
    if (!billTitle.trim() || !amount || !billDue || !householdId) return;
    createBillMutation.mutate({ householdId, title: billTitle, amount, category: billCategory || "Other", dueDateUtc: new Date(billDue).toISOString(), recurrence: billRecurrence });
  }
  const payBillMutation = useMutation({ mutationFn: payBill, onSuccess: invalidateAll });

  // budget form
  const [budgetCategory, setBudgetCategory] = useState("");
  const [budgetLimit, setBudgetLimit] = useState("");
  const setBudgetMutation = useMutation({
    mutationFn: () => setBudget(householdId, budgetCategory, parseFloat(budgetLimit)),
    onSuccess: () => { setBudgetCategory(""); setBudgetLimit(""); invalidateAll(); },
  });

  const memberName = (userId: string) => household?.members.find((m) => m.userId === userId)?.displayName ?? "—";

  return (
    <div>
      <h1>Finance</h1>
      <p className="dek">Income and expenses by category, recurring bills, and a monthly summary.</p>

      <div className="card-grid" style={{ marginBottom: 28 }}>
        <section className="card">
          <h2>This month</h2>
          <div className="finance-summary-line"><span>Income</span><span className="money-good">{money(summary?.totalIncomeThisMonth ?? 0)}</span></div>
          <div className="finance-summary-line"><span>Expenses</span><span className="money-bad">{money(summary?.totalExpenseThisMonth ?? 0)}</span></div>
        </section>
        <section className="card">
          <h2>Who paid this month</h2>
          {(summary?.spendByMember ?? []).length === 0 && <p className="empty">Nothing logged yet.</p>}
          {(summary?.spendByMember ?? []).map((s) => (
            <div key={s.userId} className="finance-summary-line"><span>{memberName(s.userId)}</span><span>{money(s.paidThisMonth)}</span></div>
          ))}
        </section>
        <section className="card">
          <h2>Budgets</h2>
          {(summary?.budgets ?? []).length === 0 && <p className="empty">No budgets set.</p>}
          {(summary?.budgets ?? []).map((b) => (
            <div key={b.category} className="finance-summary-line">
              <span>{b.category}</span>
              <span className={b.spentThisMonth > b.monthlyLimit ? "money-bad" : ""}>{money(b.spentThisMonth)} / {money(b.monthlyLimit)}</span>
            </div>
          ))}
          <form className="budget-form" onSubmit={(e) => { e.preventDefault(); if (budgetCategory && budgetLimit) setBudgetMutation.mutate(); }}>
            <input placeholder="Category" value={budgetCategory} onChange={(e) => setBudgetCategory(e.target.value)} />
            <input placeholder="Limit" type="number" value={budgetLimit} onChange={(e) => setBudgetLimit(e.target.value)} />
            <button type="submit">Set</button>
          </form>
        </section>
      </div>

      <h2 className="section-heading">Bills</h2>
      <form className="task-form" onSubmit={handleAddBill}>
        <input className="task-form-title" placeholder="Bill (e.g. Internet)" value={billTitle} onChange={(e) => setBillTitle(e.target.value)} />
        <input placeholder="Amount" type="number" value={billAmount} onChange={(e) => setBillAmount(e.target.value)} />
        <input placeholder="Category" value={billCategory} onChange={(e) => setBillCategory(e.target.value)} />
        <input type="date" value={billDue} onChange={(e) => setBillDue(e.target.value)} />
        <select value={billRecurrence} onChange={(e) => setBillRecurrence(e.target.value as BillRecurrence)}>
          <option value="None">One-off</option>
          <option value="Monthly">Monthly</option>
          <option value="Yearly">Yearly</option>
        </select>
        <button type="submit" disabled={createBillMutation.isPending}>Add bill</button>
      </form>
      <ul className="task-list" style={{ marginBottom: 28 }}>
        {(bills ?? []).map((b) => (
          <li key={b.id} className="task-row">
            <div className="task-main">
              <span className="task-title">{b.title}</span>
              <div className="task-meta">
                <span className="task-due">{new Date(b.dueDateUtc).toLocaleDateString(undefined, { month: "short", day: "numeric" })}</span>
                <span className="tag">{b.category}</span>
                <span>{money(b.amount)}</span>
              </div>
            </div>
            <button type="button" className="link-button" onClick={() => payBillMutation.mutate(b.id)}>Mark paid</button>
          </li>
        ))}
        {(bills ?? []).length === 0 && <p className="empty">No bills due.</p>}
      </ul>

      <h2 className="section-heading">Transactions</h2>
      <form className="task-form" onSubmit={handleAddTransaction}>
        <select value={txType} onChange={(e) => setTxType(e.target.value as TransactionType)}>
          <option value="Expense">Expense</option>
          <option value="Income">Income</option>
        </select>
        <input className="task-form-title" placeholder="Category" value={txCategory} onChange={(e) => setTxCategory(e.target.value)} />
        <input placeholder="Amount" type="number" value={txAmount} onChange={(e) => setTxAmount(e.target.value)} />
        <button type="submit" disabled={createTxMutation.isPending}>Add</button>
      </form>
      <ul className="task-list">
        {(transactions ?? []).map((t) => (
          <li key={t.id} className="task-row">
            <div className="task-main">
              <span className="task-title">{t.category}</span>
              <div className="task-meta">
                <span className="task-due">{new Date(t.occurredAtUtc).toLocaleDateString(undefined, { month: "short", day: "numeric" })}</span>
                <span className={t.type === "Income" ? "money-good" : "money-bad"}>{t.type === "Income" ? "+" : "-"}{money(t.amount)}</span>
              </div>
            </div>
            <button type="button" className="link-button task-delete" onClick={() => deleteTxMutation.mutate(t.id)}>Delete</button>
          </li>
        ))}
        {(transactions ?? []).length === 0 && <p className="empty">No transactions logged.</p>}
      </ul>
    </div>
  );
}
