export abstract class BaseComponent {
   protected element: HTMLElement;

   constructor(protected container: HTMLElement, protected controllerIdentifier: string) {
      this.element = this.render();
      this.appendToContainer();
      this.initialize();
   }

   protected abstract render(): HTMLElement;
   protected abstract initialize(): void;


   protected createElementFromTemplate(template: string): HTMLElement {
      const div = document.createElement('div');
      div.innerHTML = template.replaceAll('{{controllerIdentifier}}', this.controllerIdentifier).trim();

      return div.firstChild as HTMLElement;
   }

   protected appendToContainer(): void {
      this.container.appendChild(this.element);
   }


   protected bindEvent(element: HTMLElement, eventType: string, handler: (event: Event) => void): void {
      element.addEventListener(eventType, handler);
   }

   protected removeEvent(element: HTMLElement, eventType: string, handler: (event: Event) => void): void {
      element.removeEventListener(eventType, handler);
   }

   protected toggleClass(element: HTMLElement, className: string, force?: boolean): void {
      element.classList.toggle(className, force);
   }

   protected toggleAttribute(attribute: string, force?: boolean): void {
      this.element.toggleAttribute(attribute, force);
   }

   protected setStyle(style: string, value: string): void {
      this.element.style.setProperty(style, value);
   }

   protected querySelector<T extends HTMLElement>(selector: string): T | null {
      return this.element.querySelector(selector);
   }

   protected querySelectorAll<T extends HTMLElement>(selector: string): NodeListOf<T> {
      return this.element.querySelectorAll(selector);
   }

   public destroy(): void {
      this.element.remove();
   }
}