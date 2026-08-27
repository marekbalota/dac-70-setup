namespace DAC70Setup.BL.Controls;

public partial class TemplatedContentView : ContentView
{
    public static readonly BindableProperty ItemProperty = BindableProperty.Create(
        nameof(Item), typeof(object), typeof(TemplatedContentView), null, propertyChanged: OnItemOrSelectorChanged);

    public static readonly BindableProperty ItemTemplateSelectorProperty = BindableProperty.Create(
        nameof(ItemTemplateSelector), typeof(DataTemplateSelector), typeof(TemplatedContentView), null, propertyChanged: OnItemOrSelectorChanged);

    public object? Item
    {
        get => GetValue(ItemProperty);
        set => SetValue(ItemProperty, value);
    }

    public DataTemplateSelector? ItemTemplateSelector
    {
        get => (DataTemplateSelector?)GetValue(ItemTemplateSelectorProperty);
        set => SetValue(ItemTemplateSelectorProperty, value);
    }

    private static void OnItemOrSelectorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((TemplatedContentView)bindable).GenerateContent();
    }

    private void GenerateContent()
    {
        if (Item == null || ItemTemplateSelector == null)
        {
            Content = null;
            return;
        }

        DataTemplate template = ItemTemplateSelector.SelectTemplate(Item, this);

        if (template.CreateContent() is View view)
        {
            view.BindingContext = Item;
            Content = view;
        }
        else
        {
            Content = null;
        }
    }
}
