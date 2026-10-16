// Tabs keyboard support: Blazor handles Arrow/Home/End navigation in C#; this only stops those
// keys from also scrolling the page (Blazor's preventDefault cannot be conditional on the key).
const navKeys = new Set(['ArrowLeft', 'ArrowRight', 'Home', 'End']);

export function preventNavKeyScroll(tabList) {
  tabList?.addEventListener('keydown', e => {
    if (navKeys.has(e.key)) e.preventDefault();
  });
}
