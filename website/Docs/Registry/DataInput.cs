using DaisyBlazor.Docs.Examples.DataInput;
using DaisyBlazor.Validation;

namespace DaisyBlazor.Docs.Docs;

public static partial class DocRegistry
{
    private static partial IEnumerable<ComponentDoc> DataInput() =>
    [
        new("TextField", Category.DataInput,
            "A labelled text input bound to a string, with helper and error text, adornments and a multi-line mode (Lines > 1 renders a textarea). The bound value updates on every keystroke.",
            [typeof(TextField)],
            [
                new("Binding", "@bind-Value is two-way and pushes each keystroke, so anything bound to the value updates as the user types.", typeof(TextFieldBasicExample)),
                new("Adornments", "Adornment places an AdornmentIcon or AdornmentText at the start or end of the input.", typeof(TextFieldAdornmentsExample)),
                new("Multi-line", "Any Lines value above 1 swaps the input for a textarea of that many rows.", typeof(TextFieldMultilineExample)),
                new("States", "Required, Error with ErrorText, Disabled, ReadOnly, Dense, and InputType for password, email and friends.", typeof(TextFieldStatesExample)),
            ])
        {
            DaisyClass = "input",
            MudEquivalent = "MudTextField",
            Portrait = typeof(TextFieldCoverExample),
            Playground = new(typeof(TextField), "Label", "Placeholder", "HelperText", "InputType", "Lines", "Dense", "Disabled", "ReadOnly", "Required", "Error", "ErrorText")
            {
                Initial = new Dictionary<string, object?> { ["Label"] = "Project name", ["Placeholder"] = "Customer portal", ["HelperText"] = "Shown on the dashboard" },
            },
        },

        new("NumericField", Category.DataInput,
            "A number input generic over T (int, double, decimal, nullable types…), with Min, Max, Step and a display Format. Min and Max drive the native spinner bounds; they do not reject typed values, so validate on submit.",
            [typeof(NumericField<int>)],
            [
                new("Binding", "Bind any numeric type; Min, Max and Step constrain it and Format controls how it is shown.", typeof(NumericFieldBasicExample)),
                new("Types", "T can be int, decimal or a nullable type, where an empty field binds to null.", typeof(NumericFieldTypesExample)),
                new("States", "Error with ErrorText, Disabled and ReadOnly work as on TextField.", typeof(NumericFieldStatesExample)),
            ])
        {
            DaisyClass = "input",
            MudEquivalent = "MudNumericField",
            Portrait = typeof(NumericFieldCoverExample),
            Playground = new(typeof(NumericField<int>), "Label", "Value", "Min", "Max", "Step", "HelperText", "Dense", "Disabled", "ReadOnly", "Error", "ErrorText")
            {
                Initial = new Dictionary<string, object?> { ["Label"] = "Seats", ["Value"] = 3, ["Min"] = 1, ["Max"] = 10, ["HelperText"] = "Between 1 and 10" },
            },
        },

        new("Field", Category.DataInput,
            "A read-only labelled value with the same chrome as a TextField, for showing data in a form layout without making it editable.",
            [typeof(Field)],
            [
                new("Basic", "Label on top, ChildContent as the value.", typeof(FieldBasicExample)),
                new("Adornment", "An AdornmentIcon at the start or end, like the editable fields.", typeof(FieldAdornmentExample)),
                new("Echoing a bound value", "Field is handy for showing a derived value next to the input that drives it.", typeof(FieldBoundExample)),
            ])
        {
            DaisyClass = "fieldset",
            MudEquivalent = "MudField",
            Portrait = typeof(FieldCoverExample),
            Playground = new(typeof(Field), "Label", "Adornment", "AdornmentIcon")
            {
                Initial = new Dictionary<string, object?> { ["Label"] = "Plan", ["Adornment"] = Adornment.Start, ["AdornmentIcon"] = Icons.Material.Filled.CreditCard },
                ChildText = "Pro",
            },
        },

        new("FloatingLabel", Category.DataInput,
            "daisyUI's floating label: wrap any input and its label sits inside as a placeholder, then floats above once the field is focused or filled.",
            [typeof(FloatingLabel)],
            [
                new("Basic", "Wrap a native input; give it a placeholder so the label knows when the field is empty.", typeof(FloatingLabelBasicExample)),
                new("Sizes", "The label follows the input's size class (input-sm, input-lg).", typeof(FloatingLabelSizesExample)),
                new("With a select", "Works around a select, too.", typeof(FloatingLabelSelectExample)),
            ])
        {
            DaisyClass = "floating-label",
            Portrait = typeof(FloatingLabelCoverExample),
        },

        new("Select", Category.DataInput,
            "A native select generic over T, filled with SelectItem children. Supports a clear option, disabled items and multiple selection through SelectedValues.",
            [typeof(Select<string>), typeof(SelectItem<string>)],
            [
                new("Binding", "SelectItem children declare the options; Clearable adds an empty first option labelled by ClearText.", typeof(SelectBasicExample)),
                new("Typed values", "T can be any type, such as an int or an enum; an item without content shows its value.", typeof(SelectTypedExample)),
                new("Multiple selection", "MultiSelection binds the picked items to SelectedValues.", typeof(SelectMultipleExample)),
                new("States", "Disabled, Error with ErrorText, and Dense.", typeof(SelectStatesExample)),
            ])
        {
            DaisyClass = "select",
            MudEquivalent = "MudSelect",
            Portrait = typeof(SelectCoverExample),
        },

        new("Autocomplete", Category.DataInput,
            "A type-ahead input whose suggestions come from an async SearchFunc, so it works the same against an in-memory list or a remote API. Typing is debounced and stale searches are cancelled.",
            [typeof(Autocomplete<string>)],
            [
                new("Search a list", "SearchFunc gets the text and a cancellation token; suggestions appear after MinCharacters (2 by default).", typeof(AutocompleteBasicExample)),
                new("Objects and templates", "Bind a record, show it as text with ToStringFunc and style each suggestion with ItemTemplate.", typeof(AutocompleteObjectsExample)),
            ])
        {
            DaisyClass = "dropdown",
            MudEquivalent = "MudAutocomplete",
            Portrait = typeof(AutocompleteCoverExample),
        },

        new("Checkbox", Category.DataInput,
            "A daisyUI checkbox bound to a bool, with typed Color and Size. Use it for independent on/off choices; Checked is kept as an alias of Value for MudBlazor code.",
            [typeof(Checkbox)],
            [
                new("Binding", "@bind-Value is a plain bool.", typeof(CheckboxBasicExample)),
                new("Colors", "Color maps to daisyUI's checkbox-* colours.", typeof(CheckboxColorsExample)),
                new("Sizes and states", "Small, Medium and Large, plus Disabled and ReadOnly.", typeof(CheckboxSizesExample)),
                new("Select all", "A parent checkbox driving a list of children, with the count echoed below.", typeof(CheckboxListExample)),
            ])
        {
            DaisyClass = "checkbox",
            MudEquivalent = "MudCheckBox",
            Portrait = typeof(CheckboxCoverExample),
            Playground = new(typeof(Checkbox), "Label", "Value", "Color", "Size", "Disabled", "ReadOnly")
            {
                Initial = new Dictionary<string, object?> { ["Label"] = "Email me the invoice", ["Value"] = true, ["Color"] = Color.Primary },
            },
        },

        new("Switch", Category.DataInput,
            "A toggle switch bound to a bool, with the same API as Checkbox. Reach for it when the change takes effect immediately, like a setting.",
            [typeof(Switch)],
            [
                new("Binding", "@bind-Value is a plain bool.", typeof(SwitchBasicExample)),
                new("Colors", "Color maps to daisyUI's toggle-* colours.", typeof(SwitchColorsExample)),
                new("Sizes and states", "Small, Medium and Large, plus Disabled and ReadOnly.", typeof(SwitchSizesExample)),
                new("Rich label", "ChildContent replaces Label when the text needs markup.", typeof(SwitchContentExample)),
            ])
        {
            DaisyClass = "toggle",
            MudEquivalent = "MudSwitch",
            Portrait = typeof(SwitchCoverExample),
            Playground = new(typeof(Switch), "Label", "Value", "Color", "Size", "Disabled", "ReadOnly")
            {
                Initial = new Dictionary<string, object?> { ["Label"] = "Email notifications", ["Value"] = true, ["Color"] = Color.Primary },
            },
        },

        new("Radio", Category.DataInput,
            "A RadioGroup holds the selected value; each Radio inside it offers one option. Both are generic over T, and Radio infers T from its group.",
            [typeof(RadioGroup<string>), typeof(Radio<string>)],
            [
                new("Binding", "Bind the group's Value; a Radio can be Disabled on its own.", typeof(RadioBasicExample)),
                new("Typed values", "T can be any type; for value types such as int or an enum, give each radio an Option.", typeof(RadioTypedExample)),
                new("Colors and sizes", "Color and Size are set per radio.", typeof(RadioColorsExample)),
            ])
        {
            DaisyClass = "radio",
            MudEquivalent = "MudRadioGroup",
            Portrait = typeof(RadioCoverExample),
        },

        new("Range", Category.DataInput,
            "A slider bound to a double, with Min, Max and Step. It reports every movement as the thumb is dragged.",
            [typeof(DaisyBlazor.Range)],
            [
                new("Binding", "@bind-Value updates while dragging, so the read-out follows the thumb.", typeof(RangeBasicExample)),
                new("Min, max and step", "Negative ranges and fractional steps are fine.", typeof(RangeStepExample)),
                new("Colors and sizes", "Color maps to range-*; Size to range-sm and range-lg.", typeof(RangeColorsExample)),
            ])
        {
            DaisyClass = "range",
            MudEquivalent = "MudSlider",
            Portrait = typeof(RangeCoverExample),
            Playground = new(typeof(DaisyBlazor.Range), "Value", "Min", "Max", "Step", "Color", "Size")
            {
                Initial = new Dictionary<string, object?> { ["Value"] = 65d, ["Color"] = Color.Primary, ["Style"] = "width:16rem" },
            },
        },

        new("Rating", Category.DataInput,
            "A row of stars (or any daisyUI mask) bound to an int from 0 to Max. Clicking the current star again clears it; ReadOnly turns it into a display.",
            [typeof(Rating)],
            [
                new("Binding", "@bind-Value is the number of filled stars.", typeof(RatingBasicExample)),
                new("Masks, colours and sizes", "Mask takes any daisyUI mask class, such as mask-heart; Max sets the count.", typeof(RatingMasksExample)),
                new("Read-only", "ReadOnly shows a score without letting anyone change it.", typeof(RatingReadOnlyExample)),
            ])
        {
            DaisyClass = "rating",
            MudEquivalent = "MudRating",
            Portrait = typeof(RatingCoverExample),
            Playground = new(typeof(Rating), "Value", "Max", "Mask", "Color", "Size", "ReadOnly")
            {
                Initial = new Dictionary<string, object?> { ["Value"] = 4 },
            },
        },

        new("FileInput", Category.DataInput,
            "Blazor's InputFile styled as a daisyUI file-input. OnChange hands you the picked files to read or upload.",
            [typeof(FileInput)],
            [
                new("Picking a file", "OnChange receives InputFileChangeEventArgs with the file's name, size and stream.", typeof(FileInputBasicExample)),
                new("Several images", "Accept filters the picker; Multiple allows several files at once.", typeof(FileInputMultipleExample)),
                new("Styles", "Color, Size and Ghost.", typeof(FileInputStylesExample)),
            ])
        {
            DaisyClass = "file-input",
            MudEquivalent = "MudFileUpload",
            Portrait = typeof(FileInputCoverExample),
            Playground = new(typeof(FileInput), "Color", "Size", "Ghost", "Multiple", "Accept")
            {
                Initial = new Dictionary<string, object?> { ["Color"] = Color.Primary },
            },
        },

        new("DatePicker", Category.DataInput,
            "A date field bound to a DateTime?. You can type the date in the app's culture format (or DateFormat), or open the browser's native calendar from the icon.",
            [typeof(DatePicker)],
            [
                new("Binding", "@bind-Date (or @bind-Value) is a nullable DateTime; Clearable lets an emptied field bind to null.", typeof(DatePickerBasicExample)),
                new("Limits and format", "MinDate and MaxDate bound the calendar; DateFormat sets how the text reads.", typeof(DatePickerLimitsExample)),
                new("States", "Disabled, ReadOnly, and Editable=\"false\" for picking from the calendar only.", typeof(DatePickerStatesExample)),
            ])
        {
            DaisyClass = "input",
            MudEquivalent = "MudDatePicker",
            Portrait = typeof(DatePickerCoverExample),
            Playground = new(typeof(DatePicker), "Label", "Placeholder", "DateFormat", "Dense", "Disabled", "ReadOnly", "Editable", "Clearable")
            {
                Initial = new Dictionary<string, object?> { ["Label"] = "Release date" },
            },
        },

        new("ColorPicker", Category.DataInput,
            "The browser's native colour input in a labelled field, bound to a hex string. @bind-Text works too, for MudBlazor code.",
            [typeof(ColorPicker)],
            [
                new("Binding", "@bind-Value is a hex string such as #e4572e; Disabled locks it.", typeof(ColorPickerBasicExample)),
                new("Driving a preview", "The bound strings can feed any style, here a logo mock-up.", typeof(ColorPickerPreviewExample)),
            ])
        {
            DaisyClass = "input",
            MudEquivalent = "MudColorPicker",
            Portrait = typeof(ColorPickerCoverExample),
            Playground = new(typeof(ColorPicker), "Label", "Value", "Disabled", "FullWidth")
            {
                Initial = new Dictionary<string, object?> { ["Label"] = "Brand colour", ["Value"] = "#e4572e" },
            },
        },

        new("Form", Category.DataInput,
            "Form stacks fields with even spacing and keeps MudForm's Validate and ResetAsync methods so existing code compiles. Validator and ValidatorHint add daisyUI's CSS-only validation; ServerValidation and FieldError show errors returned by an API.",
            [typeof(Form), typeof(Validator), typeof(ValidatorHint), typeof(ServerValidation), typeof(FieldError)],
            [
                new("Layout", "A bound order form; the record below echoes every field as you edit.", typeof(FormBasicExample)),
                new("CSS validation", "Validator turns the browser's :invalid state into daisyUI colours and reveals its hint after the user touches the field.", typeof(FormValidatorExample)),
                new("Server errors", "ServerValidation cascades an error set; fields with a FieldName and any FieldError pick their message up.", typeof(FormServerValidationExample)),
            ])
        {
            DaisyClass = "validator",
            MudEquivalent = "MudForm",
            MigrationNote = "Form does not run validation itself: Validate() and ResetAsync() are no-ops kept for compatibility. Use EditForm with DataAnnotations, Validator, or ServerValidation for real checks.",
            Portrait = typeof(FormCoverExample),
        },
    ];
}
