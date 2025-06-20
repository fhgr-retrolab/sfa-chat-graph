import { EventEmitter } from "@angular/core";
import { Edge } from "./edge";
import { Vector } from "./vector";
import { SubGraph } from "./sub-graph";

export class Node {

  public edges: Edge[] = []
  public circleRadius: number;
  public debugVectors: Vector[] = []
  
  public readonly onChanged: EventEmitter<Node> = new EventEmitter<Node>();

  private _shouldRender: boolean = true;
  private _collapsed: boolean = false;
  private _leafsLoaded: boolean = false;
  private _hidden: boolean = false;


  constructor(
    public id: string,
    public iri: string,
    public label: string,
    public pos: Vector,
    public radius: number,
    public color: string,
    public readonly subGraph?: SubGraph,
    leafsLoaded: boolean = false,
    public isNoLeaf: boolean = false
  ) {
    this.circleRadius = radius;
    this._leafsLoaded = leafsLoaded;
  }

  getOutgoingEdges(): Edge[] {
    return this.edges.filter(edge => edge.getFrom() == this);
  }


  setHidden(shouldNeverRender: boolean){
    this._hidden = shouldNeverRender;
    this.getLeafNodes().forEach(leaf => leaf.setHidden(shouldNeverRender));
    this.onChanged?.emit(this);
  }

  isHidden(): boolean {
    return this._hidden;
  }

  areLeafsLoaded(): boolean {
    return this._leafsLoaded;
  }

  markLeafsLoaded(): void {
    this._leafsLoaded = true;
  }

  getParent(): Node|undefined {
    if(this.isLeaf()){
      return this.edges[0].getOther(this);
    }
    return undefined;
  }

  shouldRender() {
    return this._shouldRender && this._hidden == false && this.subGraph?.isHidden() == false;
  }

  setShouldRender(shouldRender: boolean){
    if(this._shouldRender != shouldRender){
      this._shouldRender = shouldRender;
      this.onChanged?.emit(this);
    } 
  }
  
  setCollapsed(collapsed: boolean) {
    if(this._collapsed != collapsed){
      this._collapsed = collapsed;
      this.getLeafNodes().forEach(l => l.setShouldRender(!collapsed));
      this.onChanged?.emit(this);
    }
  }

  isCollapsed(): boolean {
    return this._collapsed;
  }

  move(x: number, y: number) {
    this.pos.setXY(x, y);
    this.onChanged?.emit(this);
  }

  moveWithLeafs(x: number, y: number): void {
    const deltaX = x - this.pos.x;
    const deltaY = y - this.pos.y;
    this.pos.setXY(x, y)
    if (this.isLeaf() == false) {
      this.getLeafNodes().forEach(leaf => leaf.moveRelative(deltaX, deltaY));
    }
    this.onChanged?.emit(this);
  }

  moveRelativeWithLeafs(x: number, y: number): void {
    this.pos.addXYSet(x, y);
    this.getLeafNodes().forEach(leaf => leaf.moveRelative(x, y)); 
    this.onChanged?.emit(this);

  }

  moveRelative(x: number, y: number): void {
    this.pos.addXYSet(x, y);
  }

  getSiblings(): Node[] {
    return this.edges.map(edge => edge.getOther(this)!).filter(other => other.isLeaf() == false);
  }

  getLeafNodes(): Node[] {
    return this.edges.map(edge => edge.getOther(this)!).filter(other => other.isLeaf());
  }

  getVisibleLeafs() {
    return this.getLeafNodes().filter(leaf => leaf.shouldRender());
  }

  isLeaf(): boolean {
    return this.isNoLeaf == false && this.edges.length <= 1;
  }
}
