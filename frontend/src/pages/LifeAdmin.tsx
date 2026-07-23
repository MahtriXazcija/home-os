import { useState, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useHousehold } from "../hooks/useHousehold";
import {
  addShoppingItem,
  createContact,
  createDocument,
  deleteContact,
  deleteDocument,
  deleteShoppingItem,
  getContacts,
  getDocuments,
  getShoppingItems,
  setShoppingItemChecked,
} from "../api/lifeAdmin";

export default function LifeAdmin() {
  const { data: household } = useHousehold();
  const householdId = household?.id ?? "";
  const queryClient = useQueryClient();

  const { data: documents } = useQuery({ queryKey: ["documents", householdId], queryFn: () => getDocuments(householdId), enabled: !!householdId });
  const { data: contacts } = useQuery({ queryKey: ["contacts", householdId], queryFn: () => getContacts(householdId), enabled: !!householdId });
  const { data: shoppingItems } = useQuery({ queryKey: ["shopping-items", householdId], queryFn: () => getShoppingItems(householdId), enabled: !!householdId });

  const invalidate = (key: string) => queryClient.invalidateQueries({ queryKey: [key, householdId] });

  // documents
  const [docTitle, setDocTitle] = useState("");
  const [docCategory, setDocCategory] = useState("");
  const [docRenewal, setDocRenewal] = useState("");
  const createDocMutation = useMutation({
    mutationFn: createDocument,
    onSuccess: () => { setDocTitle(""); setDocCategory(""); setDocRenewal(""); invalidate("documents"); },
  });
  const deleteDocMutation = useMutation({ mutationFn: deleteDocument, onSuccess: () => invalidate("documents") });
  function handleAddDoc(e: FormEvent) {
    e.preventDefault();
    if (!docTitle.trim() || !householdId) return;
    createDocMutation.mutate({ householdId, title: docTitle, category: docCategory || "Other", renewalDateUtc: docRenewal ? new Date(docRenewal).toISOString() : null });
  }

  // contacts
  const [contactName, setContactName] = useState("");
  const [contactPhone, setContactPhone] = useState("");
  const createContactMutation = useMutation({
    mutationFn: createContact,
    onSuccess: () => { setContactName(""); setContactPhone(""); invalidate("contacts"); },
  });
  const deleteContactMutation = useMutation({ mutationFn: deleteContact, onSuccess: () => invalidate("contacts") });
  function handleAddContact(e: FormEvent) {
    e.preventDefault();
    if (!contactName.trim() || !householdId) return;
    createContactMutation.mutate({ householdId, name: contactName, phone: contactPhone || undefined });
  }

  // shopping list
  const [itemText, setItemText] = useState("");
  const addItemMutation = useMutation({
    mutationFn: (text: string) => addShoppingItem(householdId, text),
    onSuccess: () => { setItemText(""); invalidate("shopping-items"); },
  });
  const checkItemMutation = useMutation({
    mutationFn: ({ id, checked }: { id: string; checked: boolean }) => setShoppingItemChecked(id, checked),
    onSuccess: () => invalidate("shopping-items"),
  });
  const deleteItemMutation = useMutation({ mutationFn: deleteShoppingItem, onSuccess: () => invalidate("shopping-items") });

  return (
    <div>
      <h1>Life Admin</h1>
      <p className="dek">Household records, important contacts, and the shared shopping list.</p>

      <h2 className="section-heading">Documents &amp; renewals</h2>
      <form className="task-form" onSubmit={handleAddDoc}>
        <input className="task-form-title" placeholder="e.g. Car insurance" value={docTitle} onChange={(e) => setDocTitle(e.target.value)} />
        <input placeholder="Category" value={docCategory} onChange={(e) => setDocCategory(e.target.value)} />
        <input type="date" value={docRenewal} onChange={(e) => setDocRenewal(e.target.value)} title="Renewal date (optional)" />
        <button type="submit" disabled={createDocMutation.isPending}>Add</button>
      </form>
      <ul className="task-list" style={{ marginBottom: 28 }}>
        {(documents ?? []).map((d) => (
          <li key={d.id} className="task-row">
            <div className="task-main">
              <span className="task-title">{d.title}</span>
              <div className="task-meta">
                <span className="tag">{d.category}</span>
                {d.renewalDateUtc && <span className="task-due">renews {new Date(d.renewalDateUtc).toLocaleDateString(undefined, { month: "short", day: "numeric" })}</span>}
              </div>
            </div>
            <button type="button" className="link-button task-delete" onClick={() => deleteDocMutation.mutate(d.id)}>Delete</button>
          </li>
        ))}
        {(documents ?? []).length === 0 && <p className="empty">No documents yet.</p>}
      </ul>

      <h2 className="section-heading">Contacts</h2>
      <form className="task-form" onSubmit={handleAddContact}>
        <input className="task-form-title" placeholder="Name" value={contactName} onChange={(e) => setContactName(e.target.value)} />
        <input placeholder="Phone" value={contactPhone} onChange={(e) => setContactPhone(e.target.value)} />
        <button type="submit" disabled={createContactMutation.isPending}>Add</button>
      </form>
      <ul className="task-list" style={{ marginBottom: 28 }}>
        {(contacts ?? []).map((c) => (
          <li key={c.id} className="task-row">
            <div className="task-main">
              <span className="task-title">{c.name}</span>
              {c.phone && <div className="task-meta"><span>{c.phone}</span></div>}
            </div>
            <button type="button" className="link-button task-delete" onClick={() => deleteContactMutation.mutate(c.id)}>Delete</button>
          </li>
        ))}
        {(contacts ?? []).length === 0 && <p className="empty">No contacts yet.</p>}
      </ul>

      <h2 className="section-heading">Shopping list</h2>
      <form
        className="card-add-form"
        onSubmit={(e) => { e.preventDefault(); if (itemText.trim()) addItemMutation.mutate(itemText); }}
      >
        <input placeholder="Add an item…" value={itemText} onChange={(e) => setItemText(e.target.value)} />
        <button type="submit" disabled={addItemMutation.isPending}>Add</button>
      </form>
      <ul className="task-list">
        {(shoppingItems ?? []).map((item) => (
          <li key={item.id} className={`task-row${item.isChecked ? " task-done" : ""}`}>
            <input type="checkbox" checked={item.isChecked} onChange={(e) => checkItemMutation.mutate({ id: item.id, checked: e.target.checked })} />
            <div className="task-main"><span className="task-title">{item.text}</span></div>
            <button type="button" className="link-button task-delete" onClick={() => deleteItemMutation.mutate(item.id)}>Remove</button>
          </li>
        ))}
        {(shoppingItems ?? []).length === 0 && <p className="empty">List is empty.</p>}
      </ul>
    </div>
  );
}
