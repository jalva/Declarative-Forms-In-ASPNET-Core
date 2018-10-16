
import { initDynamicForm } from './forms.js'

var forms = document.querySelectorAll('.dynamic-form');
for (var i = 0; i < forms.length;i++) {
    initDynamicForm(forms[i]);
}
